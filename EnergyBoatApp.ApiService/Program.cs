var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add singleton for boat state simulation
builder.Services.AddSingleton<BoatSimulator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Simple boat data endpoint for energy service company
app.MapGet("/api/boats", (BoatSimulator simulator, double speed = 1.0) =>
{
    var boats = simulator.GetCurrentBoatStates(speed);
    return Results.Ok(boats);
})
.WithName("GetBoats")
.WithOpenApi();

// Reset boats to initial positions
app.MapPost("/api/boats/reset", (BoatSimulator simulator) =>
{
    simulator.ResetToInitialPositions();
    return Results.Ok(new { message = "Boats reset to initial positions" });
})
.WithName("ResetBoats")
.WithOpenApi();

app.MapDefaultEndpoints();

app.Run();

// Boat state simulator
class BoatSimulator
{
    private readonly List<BoatState> _boatStates;
    private readonly List<BoatState> _initialBoatStates; // Store initial positions for reset
    private readonly Dictionary<string, List<Waypoint>> _boatRoutes; // Survey routes for each boat
    private readonly Random _random = new();
    private DateTime _lastUpdate = DateTime.UtcNow;

    public BoatSimulator()
    {
        // Define survey routes (waypoints that boats follow in a loop)
        // Dock is at (51.5100, -0.1350) - all routes must navigate AROUND it
        _boatRoutes = new Dictionary<string, List<Waypoint>>
        {
            // BOAT-001: Rectangle pattern in northeast quadrant (above dock)
            ["BOAT-001"] = new List<Waypoint>
            {
                new Waypoint(51.5170, -0.1278), // Start (north of dock)
                new Waypoint(51.5250, -0.1000), // Northeast corner
                new Waypoint(51.5250, -0.1400), // Northwest corner
                new Waypoint(51.5170, -0.1400), // West (north of dock)
                new Waypoint(51.5170, -0.1278)  // Return to start
            },
            // BOAT-002: Zigzag pattern in northwest quadrant (stays north of dock)
            ["BOAT-002"] = new List<Waypoint>
            {
                new Waypoint(51.5200, -0.1450), // Start (northwest, clear of dock)
                new Waypoint(51.5250, -0.1300), // North
                new Waypoint(51.5200, -0.1200), // Northeast
                new Waypoint(51.5150, -0.1250), // East (well north of dock)
                new Waypoint(51.5200, -0.1450)  // Return to start
            },
            // BOAT-003: Triangle pattern in south quadrant (stays south of dock)
            ["BOAT-003"] = new List<Waypoint>
            {
                new Waypoint(51.5010, -0.1200), // Start (southeast, south of dock)
                new Waypoint(51.4900, -0.1250), // South-southeast
                new Waypoint(51.4900, -0.1450), // South-southwest
                new Waypoint(51.5010, -0.1450), // Southwest (south of dock)
                new Waypoint(51.5010, -0.1200)  // Return to start
            },
            // BOAT-004: Stays docked (maintenance)
            ["BOAT-004"] = new List<Waypoint>
            {
                new Waypoint(51.5100, -0.1350)  // Dock position
            }
        };

        _boatStates = new List<BoatState>
        {
            new BoatState
            {
                Id = "BOAT-001",
                Latitude = 51.5074,
                Longitude = -0.1278,
                Status = "Active",
                EnergyLevel = 85.5,
                VesselName = "Fugro Voyager",
                SurveyType = "Geophysical Survey",
                Project = "Dogger Bank Offshore Wind Farm",
                Equipment = "Multibeam Sonar, Magnetometer",
                AreaCovered = 145.5,
                Speed = "12 knots",
                CrewCount = 24,
                Conditions = "Moderate seas, 2m swell",
                Heading = 45.0,
                SpeedKnots = 12.0,
                CurrentWaypointIndex = 0,
                OriginalSpeedKnots = 12.0
            },
            new BoatState
            {
                Id = "BOAT-002",
                Latitude = 51.5154,
                Longitude = -0.1420,
                Status = "Charging",
                EnergyLevel = 42.3,
                VesselName = "Fugro Pioneer",
                SurveyType = "ROV Operations",
                Project = "Subsea Cable Route Survey",
                Equipment = "ROV, Side-scan Sonar",
                AreaCovered = 78.2,
                Speed = "Station keeping",
                CrewCount = 18,
                Conditions = "Calm, 0.5m swell",
                Heading = 0.0,
                SpeedKnots = 0.0, // Station keeping = no movement
                CurrentWaypointIndex = 0,
                OriginalSpeedKnots = 10.0 // Will resume at 10 knots when charged
            },
            new BoatState
            {
                Id = "BOAT-003",
                Latitude = 51.5010,
                Longitude = -0.1200,
                Status = "Active",
                EnergyLevel = 91.2,
                VesselName = "Fugro Navigator",
                SurveyType = "Geotechnical Survey",
                Project = "North Sea Pipeline Inspection",
                Equipment = "CPT, Seabed Sampling",
                AreaCovered = 210.8,
                Speed = "8 knots",
                CrewCount = 22,
                Conditions = "Rough seas, 3m swell",
                Heading = 135.0,
                SpeedKnots = 8.0,
                CurrentWaypointIndex = 0,
                OriginalSpeedKnots = 8.0
            },
            new BoatState
            {
                Id = "BOAT-004",
                Latitude = 51.5100,
                Longitude = -0.1350,
                Status = "Maintenance",
                EnergyLevel = 15.7,
                VesselName = "Fugro Explorer",
                SurveyType = "Standby",
                Project = "Scheduled Maintenance",
                Equipment = "Multibeam, Sub-bottom Profiler",
                AreaCovered = 0.0,
                Speed = "0 knots",
                CrewCount = 12,
                Conditions = "In port",
                Heading = 0.0,
                SpeedKnots = 0.0,
                CurrentWaypointIndex = 0,
                OriginalSpeedKnots = 0.0
            }
        };
        
        // Store deep copy of initial positions for reset functionality
        _initialBoatStates = _boatStates.Select(b => new BoatState
        {
            Id = b.Id,
            Latitude = b.Latitude,
            Longitude = b.Longitude,
            Status = b.Status,
            EnergyLevel = b.EnergyLevel,
            VesselName = b.VesselName,
            SurveyType = b.SurveyType,
            Project = b.Project,
            Equipment = b.Equipment,
            AreaCovered = b.AreaCovered,
            Speed = b.Speed,
            CrewCount = b.CrewCount,
            Conditions = b.Conditions,
            Heading = b.Heading,
            SpeedKnots = b.SpeedKnots,
            CurrentWaypointIndex = b.CurrentWaypointIndex,
            OriginalSpeedKnots = b.OriginalSpeedKnots
        }).ToList();
    }

    public IEnumerable<BoatStatus> GetCurrentBoatStates(double speedMultiplier = 1.0)
    {
        UpdateBoatPositions(speedMultiplier);
        
        return _boatStates.Select(b => new BoatStatus(
            b.Id,
            b.Latitude,
            b.Longitude,
            b.Status,
            b.EnergyLevel,
            b.VesselName,
            b.SurveyType,
            b.Project,
            b.Equipment,
            b.AreaCovered,
            b.Speed,
            b.CrewCount,
            b.Conditions
        ));
    }

    public void ResetToInitialPositions()
    {
        // Reset all boats to their initial positions
        for (int i = 0; i < _boatStates.Count; i++)
        {
            _boatStates[i].Latitude = _initialBoatStates[i].Latitude;
            _boatStates[i].Longitude = _initialBoatStates[i].Longitude;
            _boatStates[i].Heading = _initialBoatStates[i].Heading;
            _boatStates[i].Status = _initialBoatStates[i].Status;
            _boatStates[i].EnergyLevel = _initialBoatStates[i].EnergyLevel;
            _boatStates[i].SpeedKnots = _initialBoatStates[i].SpeedKnots;
            _boatStates[i].CurrentWaypointIndex = 0;
        }
        
        // Reset the last update time so positions don't jump on next update
        _lastUpdate = DateTime.UtcNow;
    }

    private void UpdateBoatPositions(double speedMultiplier = 1.0)
    {
        var now = DateTime.UtcNow;
        var elapsedSeconds = (now - _lastUpdate).TotalSeconds;
        _lastUpdate = now;

        // Use a fixed timestep for consistent movement regardless of polling rate
        // Simulate 1 second of movement per call, adjusted by speed multiplier
        var simulatedSeconds = 1.0 * speedMultiplier;

        foreach (var boat in _boatStates)
        {
            // Skip maintenance boats
            if (boat.Status == "Maintenance")
                continue;

            // CHARGING STATE LOGIC
            if (boat.Status == "Charging")
            {
                // Station keeping = no movement
                boat.SpeedKnots = 0.0;
                boat.Speed = "Station keeping";
                
                // Solar charging: ~5% per minute in simulation time
                boat.EnergyLevel += 0.083 * simulatedSeconds; // 5% per 60 seconds
                boat.EnergyLevel = Math.Min(100.0, boat.EnergyLevel);
                
                // Resume active operations when sufficiently charged
                if (boat.EnergyLevel >= 75.0)
                {
                    boat.Status = "Active";
                    boat.SpeedKnots = boat.OriginalSpeedKnots;
                    boat.Speed = $"{boat.SpeedKnots:F0} knots";
                    
                    // Point toward next waypoint
                    if (_boatRoutes.TryGetValue(boat.Id, out var route) && route.Count > 0)
                    {
                        var nextWaypoint = route[boat.CurrentWaypointIndex];
                        boat.Heading = CalculateHeading(boat.Latitude, boat.Longitude, 
                                                        nextWaypoint.Latitude, nextWaypoint.Longitude);
                    }
                }
                
                continue; // Don't move while charging
            }

            // ACTIVE STATE LOGIC
            if (boat.Status == "Active")
            {
                // Calculate distance traveled (nautical miles)
                // 1 knot = 1 nautical mile per hour
                var distanceTraveled = (boat.SpeedKnots / 3600.0) * simulatedSeconds;
                
                // Navigate toward current waypoint
                if (_boatRoutes.TryGetValue(boat.Id, out var route) && route.Count > 1)
                {
                    var targetWaypoint = route[boat.CurrentWaypointIndex];
                    var distanceToWaypoint = CalculateDistance(boat.Latitude, boat.Longitude,
                                                               targetWaypoint.Latitude, targetWaypoint.Longitude);
                    
                    // Check if reached waypoint BEFORE moving
                    // Dynamic threshold: base 0.15nm + distance traveled per update
                    // This prevents overshooting at high speeds while still detecting arrival at low speeds
                    var waypointThreshold = 0.15 + (distanceTraveled * 1.5); // nautical miles
                    
                    if (distanceToWaypoint < waypointThreshold)
                    {
                        // Move to next waypoint (loop back to start)
                        boat.CurrentWaypointIndex = (boat.CurrentWaypointIndex + 1) % route.Count;
                        targetWaypoint = route[boat.CurrentWaypointIndex];
                    }
                    
                    // Update heading to point toward waypoint
                    boat.Heading = CalculateHeading(boat.Latitude, boat.Longitude,
                                                    targetWaypoint.Latitude, targetWaypoint.Longitude);
                    
                    // Move toward waypoint
                    var headingRad = boat.Heading * Math.PI / 180.0;
                    
                    // 1 degree latitude ≈ 60 nautical miles
                    // 1 degree longitude ≈ 60 * cos(latitude) nautical miles
                    var deltaLat = distanceTraveled * Math.Cos(headingRad) / 60.0;
                    var deltaLon = distanceTraveled * Math.Sin(headingRad) / (60.0 * Math.Cos(boat.Latitude * Math.PI / 180.0));
                    
                    boat.Latitude += deltaLat;
                    boat.Longitude += deltaLon;
                }
                
                // Update area covered (faster boats cover more area)
                boat.AreaCovered += distanceTraveled * 0.05 * boat.SpeedKnots;
                
                // BATTERY DEGRADATION - faster boats drain more energy
                // Base drain rate: 0.5% per minute at 10 knots
                // Scales quadratically with speed (faster = much more drain)
                var speedFactor = boat.SpeedKnots / 10.0;
                var drainRate = 0.008 * speedFactor * speedFactor * simulatedSeconds; // % per second
                boat.EnergyLevel -= drainRate;
                boat.EnergyLevel = Math.Max(0.0, boat.EnergyLevel);
                
                // LOW BATTERY - switch to charging mode
                if (boat.EnergyLevel < 20.0)
                {
                    boat.Status = "Charging";
                    boat.Speed = "Station keeping";
                    boat.SpeedKnots = 0.0;
                    boat.Conditions = "Charging via solar panels";
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

    private class BoatState
    {
        public string Id { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; } = "";
        public double EnergyLevel { get; set; }
        public string VesselName { get; set; } = "";
        public string SurveyType { get; set; } = "";
        public string Project { get; set; } = "";
        public string Equipment { get; set; } = "";
        public double AreaCovered { get; set; }
        public string Speed { get; set; } = "";
        public int CrewCount { get; set; }
        public string Conditions { get; set; } = "";
        public double Heading { get; set; } // degrees
        public double SpeedKnots { get; set; }
        public int CurrentWaypointIndex { get; set; } // Current waypoint in route
        public double OriginalSpeedKnots { get; set; } // Speed to resume after charging
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
    string Conditions
);
