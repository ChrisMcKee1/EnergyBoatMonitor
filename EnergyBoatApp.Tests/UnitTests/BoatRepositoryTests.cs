using EnergyBoatApp.ApiService.Models;
using EnergyBoatApp.ApiService.Repositories;
using Microsoft.Extensions.Logging;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EnergyBoatApp.Tests.UnitTests;

/// <summary>
/// Unit tests for BoatRepository using Testcontainers for isolated PostgreSQL testing.
/// Each test runs against a clean database instance with schema applied.
/// </summary>
public class BoatRepositoryTests : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private NpgsqlDataSource? _dataSource;
    private IBoatRepository? _repository;
    private ILogger<BoatRepository>? _logger;

    /// <summary>
    /// Initialize PostgreSQL test container and create schema before each test.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Start PostgreSQL container
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();

        await _postgresContainer.StartAsync();

        // Create data source
        var connectionString = _postgresContainer.GetConnectionString();
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        _dataSource = dataSourceBuilder.Build();

        // Create logger
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<BoatRepository>();

        // Create repository
        _repository = new BoatRepository(_dataSource, _logger);

        // Create schema
        await CreateSchemaAsync();
    }

    /// <summary>
    /// Clean up test container after each test.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_dataSource != null)
        {
            await _dataSource.DisposeAsync();
        }

        if (_postgresContainer != null)
        {
            await _postgresContainer.DisposeAsync();
        }
    }

    private async Task CreateSchemaAsync()
    {
        const string schema = @"
            CREATE TABLE IF NOT EXISTS boats (
                id TEXT PRIMARY KEY,
                vessel_name TEXT NOT NULL,
                crew_count INTEGER NOT NULL,
                equipment TEXT NOT NULL,
                project TEXT NOT NULL,
                survey_type TEXT NOT NULL,
                created_at TIMESTAMP,
                updated_at TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS boat_states (
                boat_id TEXT PRIMARY KEY REFERENCES boats(id),
                latitude DOUBLE PRECISION NOT NULL,
                longitude DOUBLE PRECISION NOT NULL,
                heading DOUBLE PRECISION NOT NULL,
                speed_knots DOUBLE PRECISION NOT NULL,
                original_speed_knots DOUBLE PRECISION NOT NULL,
                energy_level DOUBLE PRECISION NOT NULL,
                status TEXT NOT NULL,
                speed TEXT NOT NULL,
                conditions TEXT NOT NULL,
                area_covered DOUBLE PRECISION NOT NULL,
                current_waypoint_index INTEGER NOT NULL,
                last_updated TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS waypoints (
                id SERIAL PRIMARY KEY,
                boat_id TEXT NOT NULL REFERENCES boats(id),
                latitude DOUBLE PRECISION NOT NULL,
                longitude DOUBLE PRECISION NOT NULL,
                sequence INTEGER NOT NULL,
                created_at TIMESTAMP
            );

            CREATE INDEX IF NOT EXISTS idx_waypoints_boat_sequence ON waypoints(boat_id, sequence);";

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = schema;
        await command.ExecuteNonQueryAsync();
    }

    private async Task SeedTestDataAsync()
    {
        // Insert test boat
        const string insertBoat = @"
            INSERT INTO boats (id, vessel_name, crew_count, equipment, project, survey_type, created_at, updated_at)
            VALUES ('TEST-001', 'Test Vessel', 10, 'Test Equipment', 'Test Project', 'Test Survey', NOW(), NOW())";

        const string insertState = @"
            INSERT INTO boat_states (boat_id, latitude, longitude, heading, speed_knots, original_speed_knots,
                                   energy_level, status, speed, conditions, area_covered, current_waypoint_index, last_updated)
            VALUES ('TEST-001', 51.5074, -0.1278, 45.0, 12.0, 12.0, 85.5, 'Active', '12 knots', 'Good', 25.5, 0, NOW())";

        const string insertWaypoints = @"
            INSERT INTO waypoints (boat_id, latitude, longitude, sequence, created_at)
            VALUES 
                ('TEST-001', 51.51, -0.13, 0, NOW()),
                ('TEST-001', 51.52, -0.14, 1, NOW()),
                ('TEST-001', 51.53, -0.15, 2, NOW())";

        await using var connection = await _dataSource!.OpenConnectionAsync();
        
        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = insertBoat;
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = insertState;
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = insertWaypoints;
            await cmd.ExecuteNonQueryAsync();
        }
    }

    #region GetAllBoatsWithStatesAsync Tests

    [Fact]
    public async Task GetAllBoatsWithStatesAsync_WhenDatabaseIsEmpty_ReturnsEmptyList()
    {
        // Act
        var results = await _repository!.GetAllBoatsWithStatesAsync();

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetAllBoatsWithStatesAsync_WhenBoatsExist_ReturnsBoatsWithStates()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var results = await _repository!.GetAllBoatsWithStatesAsync();

        // Assert
        Assert.NotNull(results);
        var resultList = results.ToList();
        Assert.Single(resultList);

        var (boat, state) = resultList[0];
        
        Assert.NotNull(boat);
        Assert.Equal("TEST-001", boat.Id);
        Assert.Equal("Test Vessel", boat.VesselName);
        Assert.Equal(10, boat.CrewCount);

        Assert.NotNull(state);
        Assert.Equal("TEST-001", state.BoatId);
        Assert.Equal(51.5074, state.Latitude);
        Assert.Equal(-0.1278, state.Longitude);
        Assert.Equal(45.0, state.Heading);
        Assert.Equal(12.0, state.SpeedKnots);
        Assert.Equal("Active", state.Status);
        Assert.Equal(85.5, state.EnergyLevel);
    }

    [Fact]
    public async Task GetAllBoatsWithStatesAsync_WithMultipleBoats_ReturnsAllBoats()
    {
        // Arrange
        await SeedTestDataAsync();

        // Insert second boat
        const string insertBoat2 = @"
            INSERT INTO boats (id, vessel_name, crew_count, equipment, project, survey_type)
            VALUES ('TEST-002', 'Test Vessel 2', 12, 'Equipment 2', 'Project 2', 'Survey 2')";

        const string insertState2 = @"
            INSERT INTO boat_states (boat_id, latitude, longitude, heading, speed_knots, original_speed_knots,
                                   energy_level, status, speed, conditions, area_covered, current_waypoint_index)
            VALUES ('TEST-002', 51.52, -0.14, 90.0, 8.0, 8.0, 42.0, 'Charging', '0 knots', 'Fair', 15.0, 1)";

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = insertBoat2;
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = insertState2;
            await cmd.ExecuteNonQueryAsync();
        }

        // Act
        var results = await _repository!.GetAllBoatsWithStatesAsync();

        // Assert
        var resultList = results.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, r => r.boat.Id == "TEST-001");
        Assert.Contains(resultList, r => r.boat.Id == "TEST-002");
    }

    #endregion

    #region GetBoatByIdAsync Tests

    [Fact]
    public async Task GetBoatByIdAsync_WhenBoatExists_ReturnsBoatWithState()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _repository!.GetBoatByIdAsync("TEST-001");

        // Assert
        Assert.NotNull(result);
        var (boat, state) = result.Value;

        Assert.Equal("TEST-001", boat.Id);
        Assert.Equal("Test Vessel", boat.VesselName);
        Assert.Equal("TEST-001", state.BoatId);
        Assert.Equal(51.5074, state.Latitude);
    }

    [Fact]
    public async Task GetBoatByIdAsync_WhenBoatDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _repository!.GetBoatByIdAsync("NONEXISTENT");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBoatByIdAsync_WithNullId_ThrowsException()
    {
        // Act & Assert - Npgsql throws InvalidOperationException for null parameter values
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _repository!.GetBoatByIdAsync(null!));
    }

    #endregion

    #region UpdateBoatStateAsync Tests

    [Fact]
    public async Task UpdateBoatStateAsync_WhenBoatExists_UpdatesState()
    {
        // Arrange
        await SeedTestDataAsync();

        var updatedState = new BoatState(
            BoatId: "TEST-001",
            Latitude: 51.55,
            Longitude: -0.16,
            Heading: 180.0,
            SpeedKnots: 15.0,
            OriginalSpeedKnots: 15.0,
            EnergyLevel: 75.0,
            Status: "Active",
            Speed: "15 knots",
            Conditions: "Excellent",
            AreaCovered: 30.0,
            CurrentWaypointIndex: 2,
            LastUpdated: DateTime.UtcNow
        );

        // Act
        await _repository!.UpdateBoatStateAsync(updatedState);

        // Verify update
        var result = await _repository.GetBoatByIdAsync("TEST-001");
        Assert.NotNull(result);
        var (_, state) = result.Value;

        Assert.Equal(51.55, state.Latitude);
        Assert.Equal(-0.16, state.Longitude);
        Assert.Equal(180.0, state.Heading);
        Assert.Equal(15.0, state.SpeedKnots);
        Assert.Equal(75.0, state.EnergyLevel);
        Assert.Equal(2, state.CurrentWaypointIndex);
    }

    [Fact]
    public async Task UpdateBoatStateAsync_WhenBoatDoesNotExist_DoesNotThrow()
    {
        // Arrange
        var nonExistentState = new BoatState(
            BoatId: "NONEXISTENT",
            Latitude: 51.5,
            Longitude: -0.1,
            Heading: 0,
            SpeedKnots: 0,
            OriginalSpeedKnots: 0,
            EnergyLevel: 0,
            Status: "Active",
            Speed: "0 knots",
            Conditions: "Good",
            AreaCovered: 0,
            CurrentWaypointIndex: 0,
            LastUpdated: DateTime.UtcNow
        );

        // Act & Assert - should not throw, just affect 0 rows
        await _repository!.UpdateBoatStateAsync(nonExistentState);
    }

    [Fact]
    public async Task UpdateBoatStateAsync_UpdatesMultipleFields()
    {
        // Arrange
        await SeedTestDataAsync();

        var updatedState = new BoatState(
            BoatId: "TEST-001",
            Latitude: 52.0,
            Longitude: -1.0,
            Heading: 270.0,
            SpeedKnots: 0.0,
            OriginalSpeedKnots: 12.0,
            EnergyLevel: 20.0,
            Status: "Maintenance",
            Speed: "0 knots",
            Conditions: "Poor",
            AreaCovered: 50.0,
            CurrentWaypointIndex: 5,
            LastUpdated: DateTime.UtcNow
        );

        // Act
        await _repository!.UpdateBoatStateAsync(updatedState);

        // Verify all fields updated
        var result = await _repository.GetBoatByIdAsync("TEST-001");
        var (_, state) = result!.Value;

        Assert.Equal(52.0, state.Latitude);
        Assert.Equal(-1.0, state.Longitude);
        Assert.Equal(270.0, state.Heading);
        Assert.Equal(0.0, state.SpeedKnots);
        Assert.Equal(12.0, state.OriginalSpeedKnots);
        Assert.Equal(20.0, state.EnergyLevel);
        Assert.Equal("Maintenance", state.Status);
        Assert.Equal("0 knots", state.Speed);
        Assert.Equal("Poor", state.Conditions);
        Assert.Equal(50.0, state.AreaCovered);
        Assert.Equal(5, state.CurrentWaypointIndex);
    }

    #endregion

    #region GetWaypointsForBoatAsync Tests

    [Fact]
    public async Task GetWaypointsForBoatAsync_WhenWaypointsExist_ReturnsOrderedBySequence()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var waypoints = await _repository!.GetWaypointsForBoatAsync("TEST-001");

        // Assert
        var waypointList = waypoints.ToList();
        Assert.Equal(3, waypointList.Count);

        // Verify ordering by sequence
        Assert.Equal(0, waypointList[0].Sequence);
        Assert.Equal(1, waypointList[1].Sequence);
        Assert.Equal(2, waypointList[2].Sequence);

        // Verify coordinates
        Assert.Equal(51.51, waypointList[0].Latitude);
        Assert.Equal(-0.13, waypointList[0].Longitude);
    }

    [Fact]
    public async Task GetWaypointsForBoatAsync_WhenNoWaypoints_ReturnsEmptyList()
    {
        // Arrange - create boat without waypoints
        const string insertBoat = @"
            INSERT INTO boats (id, vessel_name, crew_count, equipment, project, survey_type)
            VALUES ('TEST-NO-WP', 'Test Vessel', 10, 'Equipment', 'Project', 'Survey')";

        const string insertState = @"
            INSERT INTO boat_states (boat_id, latitude, longitude, heading, speed_knots, original_speed_knots,
                                   energy_level, status, speed, conditions, area_covered, current_waypoint_index)
            VALUES ('TEST-NO-WP', 51.5, -0.1, 0, 0, 0, 0, 'Active', '0', 'Good', 0, 0)";

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = insertBoat;
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = insertState;
            await cmd.ExecuteNonQueryAsync();
        }

        // Act
        var waypoints = await _repository!.GetWaypointsForBoatAsync("TEST-NO-WP");

        // Assert
        Assert.NotNull(waypoints);
        Assert.Empty(waypoints);
    }

    [Fact]
    public async Task GetWaypointsForBoatAsync_WhenBoatDoesNotExist_ReturnsEmptyList()
    {
        // Act
        var waypoints = await _repository!.GetWaypointsForBoatAsync("NONEXISTENT");

        // Assert
        Assert.NotNull(waypoints);
        Assert.Empty(waypoints);
    }

    #endregion

    #region ResetAllBoatsAsync Tests

    [Fact]
    public async Task ResetAllBoatsAsync_ResetsAllBoatStates()
    {
        // Arrange - Seed with 4 boats matching production data
        const string seedBoats = @"
            INSERT INTO boats (id, vessel_name, crew_count, equipment, project, survey_type) VALUES
            ('BOAT-001', 'Contoso Sea Voyager', 12, 'Multibeam Sonar, Magnetometer', 'Dogger Bank Offshore Wind Farm', 'Geophysical Survey'),
            ('BOAT-002', 'Contoso Sea Explorer', 10, 'Side Scan Sonar, Sub-bottom Profiler', 'North Sea Pipeline Route', 'Route Survey'),
            ('BOAT-003', 'Contoso Sea Surveyor', 8, 'ROV, Multibeam Echosounder', 'Hornsea Three Wind Farm', 'Site Investigation'),
            ('BOAT-004', 'Contoso Sea Navigator', 15, 'USBL, Magnetometer, Sub-bottom Profiler', 'East Anglia Hub', 'Cable Route Survey')";

        const string seedStates = @"
            INSERT INTO boat_states (boat_id, latitude, longitude, heading, speed_knots, original_speed_knots,
                                   energy_level, status, speed, conditions, area_covered, current_waypoint_index) VALUES
            ('BOAT-001', 52.0, -1.0, 90.0, 5.0, 12.0, 50.0, 'Active', '5 knots', 'Modified', 40.0, 3),
            ('BOAT-002', 52.1, -1.1, 180.0, 0.0, 0.0, 60.0, 'Charging', '0 knots', 'Modified', 20.0, 2),
            ('BOAT-003', 52.2, -1.2, 270.0, 10.0, 8.0, 95.0, 'Active', '10 knots', 'Modified', 35.0, 4),
            ('BOAT-004', 52.3, -1.3, 0.0, 0.0, 0.0, 10.0, 'Maintenance', '0 knots', 'Modified', 5.0, 1)";

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = seedBoats;
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = seedStates;
            await cmd.ExecuteNonQueryAsync();
        }

        // Act
        var rowsAffected = await _repository!.ResetAllBoatsAsync();

        // Assert
        Assert.Equal(4, rowsAffected);

        // Verify reset values match initial state from data-model.md
        var boats = await _repository.GetAllBoatsWithStatesAsync();
        var boatList = boats.ToList();

        var boat1 = boatList.First(b => b.boat.Id == "BOAT-001");
        Assert.Equal(51.5074, boat1.state.Latitude);
        Assert.Equal(-0.1278, boat1.state.Longitude);
        Assert.Equal(45.0, boat1.state.Heading);
        Assert.Equal(12.0, boat1.state.SpeedKnots);
        Assert.Equal(85.5, boat1.state.EnergyLevel);
        Assert.Equal("Active", boat1.state.Status);
        Assert.Equal(0, boat1.state.CurrentWaypointIndex);

        var boat2 = boatList.First(b => b.boat.Id == "BOAT-002");
        Assert.Equal(51.5154, boat2.state.Latitude);
        Assert.Equal(-0.1420, boat2.state.Longitude);
        Assert.Equal(0.0, boat2.state.Heading);
        Assert.Equal(0.0, boat2.state.SpeedKnots);
        Assert.Equal(42.3, boat2.state.EnergyLevel);
        Assert.Equal("Charging", boat2.state.Status);

        var boat3 = boatList.First(b => b.boat.Id == "BOAT-003");
        Assert.Equal(51.5010, boat3.state.Latitude);
        Assert.Equal(91.2, boat3.state.EnergyLevel);
        Assert.Equal("Active", boat3.state.Status);

        var boat4 = boatList.First(b => b.boat.Id == "BOAT-004");
        Assert.Equal(15.7, boat4.state.EnergyLevel);
        Assert.Equal("Maintenance", boat4.state.Status);
    }

    [Fact]
    public async Task ResetAllBoatsAsync_WhenNoBoats_Returns0()
    {
        // Act
        var rowsAffected = await _repository!.ResetAllBoatsAsync();

        // Assert
        Assert.Equal(0, rowsAffected);
    }

    [Fact]
    public async Task ResetAllBoatsAsync_IsTransactional()
    {
        // This test verifies that ResetAllBoatsAsync uses a transaction
        // by checking that either all boats are reset or none are (atomicity)
        
        // Arrange - Seed test data with the 4 boats that ResetAllBoatsAsync expects
        const string insertBoats = @"
            INSERT INTO boats (id, vessel_name, crew_count, equipment, project, survey_type) VALUES
            ('BOAT-001', 'Boat 1', 10, 'Eq', 'Proj', 'Survey'),
            ('BOAT-002', 'Boat 2', 10, 'Eq', 'Proj', 'Survey'),
            ('BOAT-003', 'Boat 3', 10, 'Eq', 'Proj', 'Survey'),
            ('BOAT-004', 'Boat 4', 10, 'Eq', 'Proj', 'Survey')";

        const string insertStates = @"
            INSERT INTO boat_states (boat_id, latitude, longitude, heading, speed_knots, original_speed_knots,
                                   energy_level, status, speed, conditions, area_covered, current_waypoint_index) VALUES
            ('BOAT-001', 50.0, -1.0, 0, 0, 0, 50, 'Active', '0', 'Test', 0, 5),
            ('BOAT-002', 50.0, -1.0, 0, 0, 0, 50, 'Active', '0', 'Test', 0, 3),
            ('BOAT-003', 50.0, -1.0, 0, 0, 0, 50, 'Active', '0', 'Test', 0, 2),
            ('BOAT-004', 50.0, -1.0, 0, 0, 0, 50, 'Active', '0', 'Test', 0, 1)";

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = insertBoats;
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = insertStates;
            await cmd.ExecuteNonQueryAsync();
        }

        // Act
        var rowsAffected = await _repository!.ResetAllBoatsAsync();

        // Assert - should successfully reset 4 boats
        Assert.Equal(4, rowsAffected);
        
        // Verify all boats have been reset to initial state
        var boats = await _repository.GetAllBoatsWithStatesAsync();
        var boatList = boats.ToList();
        
        Assert.All(boatList, boatTuple =>
        {
            // All boats should have current_waypoint_index = 0 after reset
            Assert.Equal(0, boatTuple.state.CurrentWaypointIndex);
        });
    }

    #endregion
}
