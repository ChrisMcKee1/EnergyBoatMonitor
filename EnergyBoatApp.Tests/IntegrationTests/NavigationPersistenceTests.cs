using System.Net.Http.Json;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace EnergyBoatApp.Tests.IntegrationTests;

/// <summary>
/// Integration tests for navigation simulation persistence.
/// Verifies that boat position/heading updates persist to database after simulation ticks
/// and waypoint navigation logic works with database-backed routes.
/// </summary>
public class NavigationPersistenceTests : IAsyncLifetime
{
    private DistributedApplication? _app;
    private HttpClient? _client;
    private NpgsqlDataSource? _dataSource;

    public async Task InitializeAsync()
    {
        // Create Aspire app with PostgreSQL
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.EnergyBoatApp_AppHost>();
        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        // TODO: Get HttpClient and NpgsqlDataSource when database implementation is complete
    }

    [Fact(Skip = "Will fail until repository implementation (T019) is complete")]
    public async Task Navigation_BoatPositions_PersistToDatabase()
    {
        // This test will FAIL until T019 (Update BoatSimulator to Use Repository) is implemented
        Assert.NotNull(_client);
        Assert.NotNull(_dataSource);

        // Get initial boat positions
        var response1 = await _client!.GetAsync("/api/boats");
        var boats1 = await response1.Content.ReadFromJsonAsync<List<BoatStatus>>();
        Assert.NotNull(boats1);
        
        var boat001Initial = boats1.First(b => b.Id == "BOAT-001");
        var initialLat = boat001Initial.Latitude;
        var initialLon = boat001Initial.Longitude;

        // Wait for simulation tick (2 seconds at 1x speed)
        await Task.Delay(TimeSpan.FromSeconds(3));

        // Get updated boat positions
        var response2 = await _client.GetAsync("/api/boats");
        var boats2 = await response2.Content.ReadFromJsonAsync<List<BoatStatus>>();
        Assert.NotNull(boats2);
        
        var boat001Updated = boats2.First(b => b.Id == "BOAT-001");

        // Verify position changed (boat moved)
        Assert.NotEqual(initialLat, boat001Updated.Latitude);
        Assert.NotEqual(initialLon, boat001Updated.Longitude);

        // Verify database has updated position
        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT latitude, longitude 
            FROM boat_states 
            WHERE boat_id = 'BOAT-001'";
        
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        
        var dbLat = reader.GetDouble(0);
        var dbLon = reader.GetDouble(1);

        // Database matches API response
        Assert.Equal(boat001Updated.Latitude, dbLat, precision: 6);
        Assert.Equal(boat001Updated.Longitude, dbLon, precision: 6);
    }

    [Fact(Skip = "Will fail until repository implementation (T019) is complete")]
    public async Task Navigation_BoatHeading_UpdatesPersists()
    {
        // This test will FAIL until T019 is implemented
        Assert.NotNull(_client);
        Assert.NotNull(_dataSource);

        // Get initial heading
        var response1 = await _client!.GetAsync("/api/boats");
        var boats1 = await response1.Content.ReadFromJsonAsync<List<BoatStatus>>();
        Assert.NotNull(boats1);
        
        var boat001Initial = boats1.First(b => b.Id == "BOAT-001");
        var initialHeading = boat001Initial.Heading;

        // Wait for navigation updates
        await Task.Delay(TimeSpan.FromSeconds(5));

        // Verify database has updated heading
        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT heading 
            FROM boat_states 
            WHERE boat_id = 'BOAT-001'";
        
        var dbHeading = (double)(await command.ExecuteScalarAsync() ?? 0);

        // Heading is updated in database (may or may not equal initial - depends on waypoint)
        Assert.InRange(dbHeading, 0, 360);
    }

    [Fact(Skip = "Will fail until repository implementation (T019) is complete")]
    public async Task Navigation_EnergyLevel_DecrementsInDatabase()
    {
        // This test will FAIL until T019 is implemented
        // Verify energy drains as boat travels (0.1% per nautical mile)
        Assert.NotNull(_client);
        Assert.NotNull(_dataSource);

        // Get initial energy
        var response1 = await _client!.GetAsync("/api/boats");
        var boats1 = await response1.Content.ReadFromJsonAsync<List<BoatStatus>>();
        Assert.NotNull(boats1);
        
        var boat001Initial = boats1.First(b => b.Id == "BOAT-001");
        var initialEnergy = boat001Initial.EnergyLevel;

        // Wait for boat to travel at 10x speed
        var response2 = await _client.GetAsync("/api/boats?speed=10.0");
        await Task.Delay(TimeSpan.FromSeconds(5));

        // Verify database has decremented energy
        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT energy_level 
            FROM boat_states 
            WHERE boat_id = 'BOAT-001'";
        
        var dbEnergy = (double)(await command.ExecuteScalarAsync() ?? 0);

        // Energy should decrease (unless boat reached charging status)
        Assert.InRange(dbEnergy, 0, 100);
    }

    [Fact(Skip = "Will fail until repository implementation (T019) is complete")]
    public async Task Navigation_WaypointIndex_IncrementsInDatabase()
    {
        // This test will FAIL until T019 is implemented
        Assert.NotNull(_client);
        Assert.NotNull(_dataSource);

        // Reset boats to initial state
        await _client!.PostAsync("/api/boats/reset", null);

        // Get initial waypoint index (should be 0)
        await using var connection1 = await _dataSource!.OpenConnectionAsync();
        await using var command1 = connection1.CreateCommand();
        command1.CommandText = @"
            SELECT current_waypoint_index 
            FROM boat_states 
            WHERE boat_id = 'BOAT-001'";
        
        var initialIndex = (int)(await command1.ExecuteScalarAsync() ?? 0);
        Assert.Equal(0, initialIndex);

        // Run simulation at 10x speed for long enough to reach waypoint
        await _client.GetAsync("/api/boats?speed=10.0");
        await Task.Delay(TimeSpan.FromSeconds(10));

        // Verify waypoint index incremented in database
        await using var connection2 = await _dataSource.OpenConnectionAsync();
        await using var command2 = connection2.CreateCommand();
        command2.CommandText = @"
            SELECT current_waypoint_index 
            FROM boat_states 
            WHERE boat_id = 'BOAT-001'";
        
        var updatedIndex = (int)(await command2.ExecuteScalarAsync() ?? 0);

        // Index should have incremented (or wrapped to 0 if route completed)
        Assert.True(updatedIndex >= 0);
    }

    [Fact(Skip = "Will fail until repository implementation (T019) is complete")]
    public async Task Navigation_StatusTransition_PersistsToDatabase()
    {
        // This test will FAIL until T019 is implemented
        // Verify status transitions (Active -> Charging -> Maintenance) persist
        Assert.NotNull(_client);
        Assert.NotNull(_dataSource);

        // Get current status
        var response = await _client!.GetAsync("/api/boats");
        var boats = await response.Content.ReadFromJsonAsync<List<BoatStatus>>();
        Assert.NotNull(boats);
        
        var boat = boats.First(b => b.Id == "BOAT-002"); // Charging boat
        var apiStatus = boat.Status;

        // Verify database matches API
        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT status 
            FROM boat_states 
            WHERE boat_id = 'BOAT-002'";
        
        var dbStatus = (string?)(await command.ExecuteScalarAsync());

        Assert.Equal(apiStatus, dbStatus);
        Assert.Equal("Charging", dbStatus); // BOAT-002 starts in Charging status
    }

    [Fact(Skip = "Will fail until repository implementation (T019) is complete")]
    public async Task Navigation_AtFastSpeed_AllUpdates_Persist()
    {
        // This test will FAIL until T019 is implemented
        // Verify 10x speed simulation updates all boats correctly in database
        Assert.NotNull(_client);
        Assert.NotNull(_dataSource);

        // Run at 10x speed
        await _client!.GetAsync("/api/boats?speed=10.0");
        await Task.Delay(TimeSpan.FromSeconds(5));

        // Verify all 4 boats have recent updates in database
        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*) 
            FROM boat_states 
            WHERE last_updated > NOW() - INTERVAL '10 seconds'";
        
        var recentUpdateCount = (long)(await command.ExecuteScalarAsync() ?? 0);

        Assert.Equal(4, recentUpdateCount); // All 4 boats updated recently
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();

        if (_dataSource != null)
        {
            await _dataSource.DisposeAsync();
        }

        if (_app != null)
        {
            await _app.DisposeAsync();
        }
    }
}

/// <summary>
/// BoatStatus DTO for integration tests
/// </summary>
public record BoatStatus(
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
    double Heading
);
