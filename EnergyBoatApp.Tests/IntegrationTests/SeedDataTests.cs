using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace EnergyBoatApp.Tests.IntegrationTests;

/// <summary>
/// Integration tests for seed data population.
/// Verifies that 4 boats (BOAT-001 to BOAT-004) are seeded with correct metadata, states, routes, and waypoints.
/// Per data-model.md sample data specifications.
/// </summary>
public class SeedDataTests : IAsyncLifetime
{
    private DistributedApplication? _app;
    private NpgsqlDataSource? _dataSource;

    public async Task InitializeAsync()
    {
        // Create Aspire app with PostgreSQL
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.EnergyBoatApp_AppHost>();
        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        // TODO: Get NpgsqlDataSource from DI when database implementation is complete
    }

    [Fact(Skip = "Will fail until seed data service (T018) is implemented")]
    public async Task SeedData_FourBoatsExist()
    {
        // This test will FAIL until T018 (Seed Data Service) is implemented
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = "SELECT COUNT(*) FROM boats";
        var boatCount = (long)(await command.ExecuteScalarAsync() ?? 0);

        Assert.Equal(4, boatCount);
    }

    [Fact(Skip = "Will fail until seed data service (T018) is implemented")]
    public async Task SeedData_Boat001_HasCorrectMetadata()
    {
        // This test will FAIL until T018 is implemented
        // Verify BOAT-001: Contoso Sea Voyager, 24 crew, Geophysical Survey
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = @"
            SELECT vessel_name, crew_count, survey_type, project
            FROM boats 
            WHERE id = 'BOAT-001'";
        
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        
        Assert.Equal("Contoso Sea Voyager", reader.GetString(0));
        Assert.Equal(24, reader.GetInt32(1));
        Assert.Equal("Geophysical Survey", reader.GetString(2));
        Assert.Equal("Dogger Bank Offshore Wind Farm", reader.GetString(3));
    }

    [Fact(Skip = "Will fail until seed data service (T018) is implemented")]
    public async Task SeedData_AllBoats_HaveInitialStates()
    {
        // This test will FAIL until T018 is implemented
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = "SELECT COUNT(*) FROM boat_states";
        var stateCount = (long)(await command.ExecuteScalarAsync() ?? 0);

        Assert.Equal(4, stateCount);
    }

    [Fact(Skip = "Will fail until seed data service (T018) is implemented")]
    public async Task SeedData_Boat001_HasCorrectInitialState()
    {
        // This test will FAIL until T018 is implemented
        // Per data-model.md: BOAT-001 starts at 51.5074, -0.1278, heading 45, energy 85.5, status Active
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = @"
            SELECT latitude, longitude, heading, energy_level, status, speed_knots
            FROM boat_states 
            WHERE boat_id = 'BOAT-001'";
        
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        
        Assert.Equal(51.5074, reader.GetDouble(0), precision: 4);
        Assert.Equal(-0.1278, reader.GetDouble(1), precision: 4);
        Assert.Equal(45.0, reader.GetDouble(2), precision: 1);
        Assert.Equal(85.5, reader.GetDouble(3), precision: 1);
        Assert.Equal("Active", reader.GetString(4));
        Assert.Equal(12.0, reader.GetDouble(5), precision: 1);
    }

    [Fact(Skip = "Will fail until seed data service (T018) is implemented")]
    public async Task SeedData_AllBoats_HaveRoutes()
    {
        // This test will FAIL until T018 is implemented
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = "SELECT COUNT(*) FROM routes";
        var routeCount = (long)(await command.ExecuteScalarAsync() ?? 0);

        Assert.Equal(4, routeCount);
    }

    [Fact(Skip = "Will fail until seed data service (T018) is implemented")]
    public async Task SeedData_Boat001_HasRectanglePatternRoute()
    {
        // This test will FAIL until T018 is implemented
        // Per data-model.md: BOAT-001 has "Rectangle Pattern - NE Quadrant"
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = @"
            SELECT route_name 
            FROM routes 
            WHERE boat_id = 'BOAT-001'";
        
        var routeName = (string?)(await command.ExecuteScalarAsync());
        
        Assert.NotNull(routeName);
        Assert.Contains("Rectangle", routeName);
    }

    [Fact(Skip = "Will fail until seed data service (T018) is implemented")]
    public async Task SeedData_WaypointsExist()
    {
        // This test will FAIL until T018 is implemented
        // Per data-model.md: Each boat should have waypoints (15-20 total)
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = "SELECT COUNT(*) FROM waypoints";
        var waypointCount = (long)(await command.ExecuteScalarAsync() ?? 0);

        Assert.True(waypointCount >= 15); // At least 15 waypoints total
        Assert.True(waypointCount <= 20); // No more than 20 waypoints
    }

    [Fact(Skip = "Will fail until seed data service (T018) is implemented")]
    public async Task SeedData_Boat001_HasSequencedWaypoints()
    {
        // This test will FAIL until T018 is implemented
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = @"
            SELECT COUNT(*), MIN(sequence), MAX(sequence)
            FROM waypoints 
            WHERE boat_id = 'BOAT-001'";
        
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        
        var waypointCount = reader.GetInt64(0);
        var minSequence = reader.GetInt32(1);
        var maxSequence = reader.GetInt32(2);

        Assert.True(waypointCount > 0);
        Assert.Equal(0, minSequence); // Sequence starts at 0
        Assert.Equal(waypointCount - 1, maxSequence); // Sequence is 0-indexed
    }

    [Fact(Skip = "Will fail until seed data service (T018) is implemented")]
    public async Task SeedData_AllWaypoints_HaveValidCoordinates()
    {
        // This test will FAIL until T018 is implemented
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = @"
            SELECT COUNT(*) 
            FROM waypoints 
            WHERE latitude BETWEEN -90 AND 90 
            AND longitude BETWEEN -180 AND 180";
        
        var validCount = (long)(await command.ExecuteScalarAsync() ?? 0);

        command.CommandText = "SELECT COUNT(*) FROM waypoints";
        var totalCount = (long)(await command.ExecuteScalarAsync() ?? 0);

        Assert.Equal(totalCount, validCount); // All waypoints must have valid coordinates
    }

    [Fact(Skip = "Will fail until seed data service (T018) is implemented")]
    public async Task SeedData_NoBoats_HaveDuplicateWaypointSequences()
    {
        // This test will FAIL until T018 is implemented
        // Verify UNIQUE (boat_id, sequence) constraint is enforced
        Assert.NotNull(_dataSource);

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        
        command.CommandText = @"
            SELECT boat_id, sequence, COUNT(*) 
            FROM waypoints 
            GROUP BY boat_id, sequence 
            HAVING COUNT(*) > 1";
        
        await using var reader = await command.ExecuteReaderAsync();
        Assert.False(await reader.ReadAsync()); // Should return no rows (no duplicates)
    }

    public async Task DisposeAsync()
    {
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
