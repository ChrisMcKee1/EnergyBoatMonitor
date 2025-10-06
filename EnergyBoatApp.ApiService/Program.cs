using EnergyBoatApp.ApiService.Services;
using EnergyBoatApp.ApiService.Repositories;
using EnergyBoatApp.ApiService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add PostgreSQL database connection with resilience configuration
builder.AddNpgsqlDataSource(
    connectionName: "ContosoSeaDB",
    configureDataSourceBuilder: dataSourceBuilder =>
    {
        // Configure connection pooling for high-load scenarios (supports 10x simulation speed)
        // Default connection string will be enhanced with these parameters via the builder
        dataSourceBuilder.ConnectionStringBuilder.MaxPoolSize = 100;
        dataSourceBuilder.ConnectionStringBuilder.MinPoolSize = 10;
        
        // Configure timeouts for reliability
        dataSourceBuilder.ConnectionStringBuilder.Timeout = 30;      // Connection timeout (seconds)
        dataSourceBuilder.ConnectionStringBuilder.CommandTimeout = 60; // Command timeout (seconds)
        
        // Connection lifecycle settings
        dataSourceBuilder.ConnectionStringBuilder.ConnectionIdleLifetime = 300; // Close idle connections after 5 minutes
        dataSourceBuilder.ConnectionStringBuilder.ConnectionPruningInterval = 10; // Check for idle connections every 10 seconds
    });

// Add database initialization service (runs schema migration on startup)
builder.Services.AddHostedService<DatabaseInitializationService>();

// Add seed data service (populates initial data if database is empty)
builder.Services.AddHostedService<SeedDataService>();

// Add repository for database access
builder.Services.AddScoped<IBoatRepository, BoatRepository>();

// Add singleton for boat state simulation
builder.Services.AddSingleton<BoatSimulator>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("ContosoSeaDB") 
               ?? throw new InvalidOperationException("ContosoSeaDB connection string not configured"));

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Simple boat data endpoint for energy service company
app.MapGet("/api/boats", async (BoatSimulator simulator, double speed = 1.0) =>
{
    var boats = await simulator.GetCurrentBoatStatesAsync(speed);
    return Results.Ok(boats);
})
.WithName("GetBoats")
.WithOpenApi();

// Reset boats to initial positions
app.MapPost("/api/boats/reset", async (BoatSimulator simulator) =>
{
    await simulator.ResetToInitialPositionsAsync();
    return Results.Ok(new { message = "Boats reset to initial positions" });
})
.WithName("ResetBoats")
.WithOpenApi();

// Health check endpoint
app.MapHealthChecks("/health");

app.MapDefaultEndpoints();

app.Run();

// Boat state simulator - now uses database via repository
class BoatSimulator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Random _random = new();
    private DateTime _lastUpdate = DateTime.UtcNow;

    public BoatSimulator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<IEnumerable<BoatStatus>> GetCurrentBoatStatesAsync(double speedMultiplier = 1.0)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBoatRepository>();
        
        // Load current boat states from database
        var boatsData = (await repository.GetAllBoatsWithStatesAsync()).ToList();
        
        // Update positions based on simulation
        await UpdateBoatPositionsAsync(boatsData, repository, speedMultiplier);
        
        // Convert to response DTOs
        return boatsData.Select(bd => new BoatStatus(
            bd.boat.Id,
            bd.state.Latitude,
            bd.state.Longitude,
            bd.state.Status,
            bd.state.EnergyLevel,
            bd.boat.VesselName,
            bd.boat.SurveyType,
            bd.boat.Project,
            bd.boat.Equipment,
            bd.state.AreaCovered,
            bd.state.Speed,
            bd.boat.CrewCount,
            bd.state.Conditions,
            bd.state.Heading
        ));
    }

    public async Task ResetToInitialPositionsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBoatRepository>();
        
        await repository.ResetAllBoatsAsync();
        
        // Reset the last update time so positions don't jump on next update
        _lastUpdate = DateTime.UtcNow;
    }

    private async Task UpdateBoatPositionsAsync(List<(Boat boat, BoatState state)> boatsData, IBoatRepository repository, double speedMultiplier = 1.0)
    {
        var now = DateTime.UtcNow;
        var elapsedSeconds = (now - _lastUpdate).TotalSeconds;
        _lastUpdate = now;

        // Use a fixed timestep for consistent movement regardless of polling rate
        // Simulate 1 second of movement per call, adjusted by speed multiplier
        var simulatedSeconds = 1.0 * speedMultiplier;

        foreach (var (boat, state) in boatsData)
        {
            // Skip maintenance boats
            if (state.Status == "Maintenance")
                continue;

            // Load waypoints for this boat
            var waypoints = (await repository.GetWaypointsForBoatAsync(boat.Id)).ToList();
            if (waypoints.Count == 0) continue;

            // CHARGING STATE LOGIC
            if (state.Status == "Charging")
            {
                // Solar charging: ~5% per minute in simulation time
                var newEnergyLevel = Math.Min(100.0, state.EnergyLevel + (0.083 * simulatedSeconds));
                
                // Resume active operations when sufficiently charged
                if (newEnergyLevel >= 75.0)
                {
                    // Point toward next waypoint
                    var currentWaypoint = waypoints[(int)state.CurrentWaypointIndex];
                    var newHeading = CalculateHeading(state.Latitude, state.Longitude, 
                                                     currentWaypoint.Latitude, currentWaypoint.Longitude);
                    
                    // Create updated state (resuming active operations)
                    var updatedState = state with
                    {
                        Status = "Active",
                        Speed = $"{state.SpeedKnots:F0} knots",
                        EnergyLevel = newEnergyLevel,
                        Heading = newHeading,
                        LastUpdated = now
                    };
                    
                    await repository.UpdateBoatStateAsync(updatedState);
                }
                else
                {
                    // Still charging (station keeping)
                    var updatedState = state with
                    {
                        Speed = "Station keeping",
                        EnergyLevel = newEnergyLevel,
                        LastUpdated = now
                    };
                    
                    await repository.UpdateBoatStateAsync(updatedState);
                }
                
                continue; // Don't move while charging
            }

            // ACTIVE STATE LOGIC
            if (state.Status == "Active")
            {
                // Calculate distance traveled (nautical miles)
                // 1 knot = 1 nautical mile per hour
                var distanceTraveled = (state.SpeedKnots / 3600.0) * simulatedSeconds;
                
                // Navigate toward current waypoint
                if (waypoints.Count > 1)
                {
                    var currentWaypointIndex = state.CurrentWaypointIndex;
                    var targetWaypoint = waypoints[currentWaypointIndex];
                    var distanceToWaypoint = CalculateDistance(state.Latitude, state.Longitude,
                                                               targetWaypoint.Latitude, targetWaypoint.Longitude);
                    
                    // Check if reached waypoint BEFORE moving
                    // Dynamic threshold: base 0.15nm + distance traveled per update
                    // This prevents overshooting at high speeds while still detecting arrival at low speeds
                    var waypointThreshold = 0.15 + (distanceTraveled * 1.5); // nautical miles
                    
                    if (distanceToWaypoint < waypointThreshold)
                    {
                        // Move to next waypoint (loop back to start)
                        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
                        targetWaypoint = waypoints[currentWaypointIndex];
                    }
                    
                    // Update heading to point toward waypoint
                    var newHeading = CalculateHeading(state.Latitude, state.Longitude,
                                                     targetWaypoint.Latitude, targetWaypoint.Longitude);
                    
                    // Move toward waypoint
                    var headingRad = newHeading * Math.PI / 180.0;
                    
                    // 1 degree latitude ≈ 60 nautical miles
                    // 1 degree longitude ≈ 60 * cos(latitude) nautical miles
                    var deltaLat = distanceTraveled * Math.Cos(headingRad) / 60.0;
                    var deltaLon = distanceTraveled * Math.Sin(headingRad) / (60.0 * Math.Cos(state.Latitude * Math.PI / 180.0));
                    
                    var newLatitude = state.Latitude + deltaLat;
                    var newLongitude = state.Longitude + deltaLon;
                
                    // Update area covered (faster boats cover more area)
                    var newAreaCovered = state.AreaCovered + (distanceTraveled * 0.05 * state.SpeedKnots);
                    
                    // BATTERY DEGRADATION - faster boats drain more energy
                    // Base drain rate: 0.5% per minute at 10 knots
                    // Scales quadratically with speed (faster = much more drain)
                    var speedFactor = state.SpeedKnots / 10.0;
                    var drainRate = 0.008 * speedFactor * speedFactor * simulatedSeconds; // % per second
                    var newEnergyLevel = Math.Max(0.0, state.EnergyLevel - drainRate);
                    
                    // LOW BATTERY - switch to charging mode
                    if (newEnergyLevel < 20.0)
                    {
                        var updatedState = state with
                        {
                            Latitude = newLatitude,
                            Longitude = newLongitude,
                            Heading = newHeading,
                            CurrentWaypointIndex = currentWaypointIndex,
                            AreaCovered = newAreaCovered,
                            EnergyLevel = newEnergyLevel,
                            Status = "Charging",
                            Speed = "Station keeping",
                            Conditions = "Charging via solar panels",
                            LastUpdated = now
                        };
                        
                        await repository.UpdateBoatStateAsync(updatedState);
                    }
                    else
                    {
                        var updatedState = state with
                        {
                            Latitude = newLatitude,
                            Longitude = newLongitude,
                            Heading = newHeading,
                            CurrentWaypointIndex = currentWaypointIndex,
                            AreaCovered = newAreaCovered,
                            EnergyLevel = newEnergyLevel,
                            LastUpdated = now
                        };
                        
                        await repository.UpdateBoatStateAsync(updatedState);
                    }
                }
            }
        }
    }
    
    // Calculate distance between two lat/lon points in nautical miles
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLon = (lon2 - lon1) * Math.PI / 180.0;
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distanceKm = 6371 * c; // Earth radius in km
        
        return distanceKm * 0.539957; // Convert km to nautical miles
    }
    
    // Calculate heading (bearing) from one point to another in degrees
    private double CalculateHeading(double lat1, double lon1, double lat2, double lon2)
    {
        var dLon = (lon2 - lon1) * Math.PI / 180.0;
        var lat1Rad = lat1 * Math.PI / 180.0;
        var lat2Rad = lat2 * Math.PI / 180.0;
        
        var y = Math.Sin(dLon) * Math.Cos(lat2Rad);
        var x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(dLon);
        
        var heading = Math.Atan2(y, x) * 180.0 / Math.PI;
        
        return (heading + 360.0) % 360.0; // Normalize to 0-360
    }
}

// Waypoint for survey routes
record Waypoint(double Latitude, double Longitude);

record BoatStatus(
    string Id,
    double Latitude,
    double Longitude,
    string Status,
    double EnergyLevel,
    string VesselName,
    string SurveyType,
    string Project,
    string Equipment,
    double AreaCovered,
    string Speed,
    int CrewCount,
    string Conditions,
    double Heading  // Add heading to the response
);

// Make Program class accessible to tests
public partial class Program { }
