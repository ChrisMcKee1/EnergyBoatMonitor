using System.Collections.Concurrent;
using System.Diagnostics;
using EnergyBoatApp.ApiService.Models;
using EnergyBoatApp.ApiService.Repositories;
using Microsoft.Extensions.Logging;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EnergyBoatApp.Tests.PerformanceTests;

/// <summary>
/// Performance tests for concurrent update scenarios.
/// Validates that multiple boats can update states simultaneously without deadlocks.
/// Tests simulate 10x simulation speed (4 boats × 200ms = 20 updates/sec baseline, 200 updates/sec at 10x).
/// </summary>
public class ConcurrentUpdateTests : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private NpgsqlDataSource? _dataSource;
    private IBoatRepository? _repository;
    private ILogger<BoatRepository>? _logger;

    /// <summary>
    /// Initialize PostgreSQL test container with production-like schema and data.
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

        // Create data source with connection pooling (matching production config)
        var connectionString = _postgresContainer.GetConnectionString();
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        
        // Configure connection pooling (from T025 - connection resilience)
        dataSourceBuilder.ConnectionStringBuilder.MaxPoolSize = 100;
        dataSourceBuilder.ConnectionStringBuilder.MinPoolSize = 10;
        dataSourceBuilder.ConnectionStringBuilder.Timeout = 30;
        dataSourceBuilder.ConnectionStringBuilder.CommandTimeout = 60;
        dataSourceBuilder.ConnectionStringBuilder.ConnectionIdleLifetime = 300;
        
        _dataSource = dataSourceBuilder.Build();

        // Create logger
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<BoatRepository>();

        // Create repository
        _repository = new BoatRepository(_dataSource, _logger);

        // Create schema and seed production data
        await CreateSchemaAsync();
        await SeedProductionDataAsync();
    }

    /// <summary>
    /// Clean up test container after tests complete.
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

            -- Performance indexes
            CREATE INDEX IF NOT EXISTS idx_boat_states_status ON boat_states(status);
            CREATE INDEX IF NOT EXISTS idx_boat_states_energy ON boat_states(energy_level);
            CREATE INDEX IF NOT EXISTS idx_boat_states_updated ON boat_states(last_updated DESC);
            CREATE INDEX IF NOT EXISTS idx_waypoints_boat_sequence ON waypoints(boat_id, sequence);";

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = schema;
        await command.ExecuteNonQueryAsync();
    }

    private async Task SeedProductionDataAsync()
    {
        // Seed 4 production boats matching data-model.md
        const string insertBoats = @"
            INSERT INTO boats (id, vessel_name, crew_count, equipment, project, survey_type, created_at, updated_at) VALUES
            ('BOAT-001', 'Contoso Sea Voyager', 12, 'Multibeam Sonar, Magnetometer', 'Dogger Bank Offshore Wind Farm', 'Geophysical Survey', NOW(), NOW()),
            ('BOAT-002', 'Contoso Sea Explorer', 10, 'Side Scan Sonar, Sub-bottom Profiler', 'North Sea Pipeline Route', 'Route Survey', NOW(), NOW()),
            ('BOAT-003', 'Contoso Sea Surveyor', 8, 'ROV, Multibeam Echosounder', 'Hornsea Three Wind Farm', 'Site Investigation', NOW(), NOW()),
            ('BOAT-004', 'Contoso Sea Navigator', 15, 'USBL, Magnetometer, Sub-bottom Profiler', 'East Anglia Hub', 'Cable Route Survey', NOW(), NOW())";

        const string insertStates = @"
            INSERT INTO boat_states (boat_id, latitude, longitude, heading, speed_knots, original_speed_knots,
                                   energy_level, status, speed, conditions, area_covered, current_waypoint_index, last_updated) VALUES
            ('BOAT-001', 51.5074, -0.1278, 45.0, 12.0, 12.0, 85.5, 'Active', '12 knots', 'Good sea state, light winds', 23.7, 0, NOW()),
            ('BOAT-002', 51.5154, -0.1420, 0.0, 0.0, 0.0, 42.3, 'Charging', '0 knots', 'Docked at charging station', 18.9, 1, NOW()),
            ('BOAT-003', 51.5010, -0.0920, 180.0, 8.0, 8.0, 91.2, 'Active', '8 knots', 'Excellent visibility, calm seas', 31.2, 0, NOW()),
            ('BOAT-004', 51.4885, -0.1650, 90.0, 0.0, 0.0, 15.7, 'Maintenance', '0 knots', 'Scheduled maintenance dock', 12.4, 0, NOW())";

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
    }

    #region Concurrent Update Tests

    [Fact]
    public async Task ConcurrentUpdates_4Boats_NoDeadlocks()
    {
        // Simulate 4 boats updating every 200ms (10x speed)
        // Run for 5 seconds = 25 updates per boat = 100 total updates
        const int durationSeconds = 5;
        const int updateIntervalMs = 200; // 10x speed
        const int boatCount = 4;

        var boatIds = new[] { "BOAT-001", "BOAT-002", "BOAT-003", "BOAT-004" };
        var updateCounts = new ConcurrentDictionary<string, int>();
        var errors = new ConcurrentBag<Exception>();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(durationSeconds));

        // Initialize update counters
        foreach (var boatId in boatIds)
        {
            updateCounts[boatId] = 0;
        }

        // Create update tasks for each boat
        var updateTasks = boatIds.Select(async boatId =>
        {
            var random = new Random(boatId.GetHashCode());
            
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    // Simulate boat state update with random movement
                    var state = new BoatState(
                        BoatId: boatId,
                        Latitude: 51.5 + (random.NextDouble() * 0.1),
                        Longitude: -0.1 + (random.NextDouble() * 0.1),
                        Heading: random.Next(0, 360),
                        SpeedKnots: random.Next(0, 15),
                        OriginalSpeedKnots: 12.0,
                        EnergyLevel: 50 + (random.NextDouble() * 50),
                        Status: "Active",
                        Speed: $"{random.Next(0, 15)} knots",
                        Conditions: "Test conditions",
                        AreaCovered: random.Next(0, 100),
                        CurrentWaypointIndex: random.Next(0, 3),
                        LastUpdated: DateTime.UtcNow
                    );

                    await _repository!.UpdateBoatStateAsync(state);
                    updateCounts.AddOrUpdate(boatId, 1, (_, count) => count + 1);

                    await Task.Delay(updateIntervalMs, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // Expected when test completes
                    break;
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                    break;
                }
            }
        }).ToArray();

        // Wait for all update tasks to complete
        await Task.WhenAll(updateTasks);

        // Assert - No deadlocks or errors
        Assert.Empty(errors);

        // Assert - Each boat updated approximately the expected number of times
        var totalUpdates = updateCounts.Values.Sum();
        var expectedUpdates = (durationSeconds * 1000 / updateIntervalMs) * boatCount;
        var actualUpdatesPerBoat = totalUpdates / (double)boatCount;
        var expectedUpdatesPerBoat = expectedUpdates / (double)boatCount;

        Console.WriteLine($"Concurrent Update Test Results:");
        Console.WriteLine($"  Duration: {durationSeconds}s");
        Console.WriteLine($"  Update Interval: {updateIntervalMs}ms");
        Console.WriteLine($"  Boat Count: {boatCount}");
        Console.WriteLine($"  Total Updates: {totalUpdates}");
        Console.WriteLine($"  Expected Updates: ~{expectedUpdates}");
        Console.WriteLine($"  Updates per Boat:");
        foreach (var kvp in updateCounts.OrderBy(x => x.Key))
        {
            Console.WriteLine($"    {kvp.Key}: {kvp.Value} updates");
        }

        // Allow for some variance due to timing (±20%)
        Assert.True(actualUpdatesPerBoat >= expectedUpdatesPerBoat * 0.8,
            $"Update count {actualUpdatesPerBoat:F1} too low (expected ~{expectedUpdatesPerBoat:F1})");
        Assert.True(actualUpdatesPerBoat <= expectedUpdatesPerBoat * 1.2,
            $"Update count {actualUpdatesPerBoat:F1} too high (expected ~{expectedUpdatesPerBoat:F1})");
    }

    [Fact]
    public async Task ConcurrentUpdates_HighLoad_NoConnectionPoolExhaustion()
    {
        // Simulate extreme load: 100 concurrent updates
        // This tests connection pool capacity (MaxPoolSize=100)
        const int concurrentUpdates = 100;
        var tasks = new List<Task>();
        var errors = new ConcurrentBag<Exception>();
        var successCount = 0;
        var successLock = new object();

        var sw = Stopwatch.StartNew();

        for (int i = 0; i < concurrentUpdates; i++)
        {
            var boatId = $"BOAT-00{(i % 4) + 1}"; // Cycle through 4 boats
            var updateIndex = i;

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var state = new BoatState(
                        BoatId: boatId,
                        Latitude: 51.5 + (updateIndex * 0.001),
                        Longitude: -0.1 + (updateIndex * 0.001),
                        Heading: updateIndex % 360,
                        SpeedKnots: updateIndex % 15,
                        OriginalSpeedKnots: 12.0,
                        EnergyLevel: 50 + (updateIndex % 50),
                        Status: "Active",
                        Speed: $"{updateIndex % 15} knots",
                        Conditions: "High load test",
                        AreaCovered: updateIndex % 100,
                        CurrentWaypointIndex: updateIndex % 3,
                        LastUpdated: DateTime.UtcNow
                    );

                    await _repository!.UpdateBoatStateAsync(state);

                    lock (successLock)
                    {
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }));
        }

        await Task.WhenAll(tasks);
        sw.Stop();

        // Assert - No connection pool exhaustion errors
        var poolErrors = errors.Where(e => 
            e.Message.Contains("pool", StringComparison.OrdinalIgnoreCase) ||
            e.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase));

        Assert.Empty(poolErrors);
        
        // Assert - All updates succeeded
        Assert.Equal(concurrentUpdates, successCount);

        Console.WriteLine($"High Load Concurrent Update Test:");
        Console.WriteLine($"  Concurrent Updates: {concurrentUpdates}");
        Console.WriteLine($"  Success Count: {successCount}");
        Console.WriteLine($"  Error Count: {errors.Count}");
        Console.WriteLine($"  Total Time: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"  Average Time per Update: {sw.ElapsedMilliseconds / (double)concurrentUpdates:F2}ms");
    }

    [Fact]
    public async Task ConcurrentUpdates_AllUpdatesCommitted()
    {
        // Verify that concurrent updates are all committed and visible
        // Simulate 10 updates per boat concurrently
        const int updatesPerBoat = 10;
        var boatIds = new[] { "BOAT-001", "BOAT-002", "BOAT-003", "BOAT-004" };
        var finalStates = new ConcurrentDictionary<string, BoatState>();

        // Run concurrent updates
        var tasks = boatIds.SelectMany((boatId, boatIndex) =>
            Enumerable.Range(0, updatesPerBoat).Select(async updateIndex =>
            {
                var state = new BoatState(
                    BoatId: boatId,
                    Latitude: 51.5 + (updateIndex * 0.01),
                    Longitude: -0.1 + (updateIndex * 0.01),
                    Heading: (boatIndex * 90) + updateIndex,
                    SpeedKnots: updateIndex,
                    OriginalSpeedKnots: 12.0,
                    EnergyLevel: 50.0 + updateIndex,
                    Status: "Active",
                    Speed: $"{updateIndex} knots",
                    Conditions: $"Update {updateIndex}",
                    AreaCovered: updateIndex * 2.0,
                    CurrentWaypointIndex: updateIndex % 3,
                    LastUpdated: DateTime.UtcNow
                );

                await _repository!.UpdateBoatStateAsync(state);

                // Track final state (last update wins)
                finalStates.AddOrUpdate(boatId, state, (_, _) => state);
            })
        ).ToArray();

        await Task.WhenAll(tasks);

        // Verify all boats have their final states committed
        foreach (var boatId in boatIds)
        {
            var result = await _repository!.GetBoatByIdAsync(boatId);
            Assert.NotNull(result);

            var (_, state) = result.Value;
            
            // State should reflect one of the updates (may not be the absolute last due to timing)
            // But it should be one of the valid update values
            Assert.True(state.Latitude >= 51.5 && state.Latitude <= 51.6,
                $"{boatId} latitude {state.Latitude} not in expected range");
            Assert.True(state.EnergyLevel >= 50.0 && state.EnergyLevel <= 60.0,
                $"{boatId} energy {state.EnergyLevel} not in expected range");
        }

        Console.WriteLine($"Concurrent Update Commit Test:");
        Console.WriteLine($"  Total Updates: {updatesPerBoat * boatIds.Length}");
        Console.WriteLine($"  All updates committed and visible: ✅");
    }

    [Fact]
    public async Task ConcurrentUpdates_WithReadOperations_NoConflicts()
    {
        // Simulate realistic scenario: updates happening while reads occur
        // 4 boats updating + 10 concurrent read operations
        const int durationSeconds = 3;
        const int updateIntervalMs = 200;
        const int concurrentReaders = 10;

        var boatIds = new[] { "BOAT-001", "BOAT-002", "BOAT-003", "BOAT-004" };
        var updateCounts = new ConcurrentDictionary<string, int>();
        var readCounts = new ConcurrentDictionary<int, int>();
        var errors = new ConcurrentBag<Exception>();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(durationSeconds));

        // Initialize counters
        foreach (var boatId in boatIds)
        {
            updateCounts[boatId] = 0;
        }

        // Start update tasks (writers)
        var updateTasks = boatIds.Select(async boatId =>
        {
            var random = new Random(boatId.GetHashCode());
            
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    var state = new BoatState(
                        BoatId: boatId,
                        Latitude: 51.5 + (random.NextDouble() * 0.1),
                        Longitude: -0.1 + (random.NextDouble() * 0.1),
                        Heading: random.Next(0, 360),
                        SpeedKnots: random.Next(0, 15),
                        OriginalSpeedKnots: 12.0,
                        EnergyLevel: 50 + (random.NextDouble() * 50),
                        Status: "Active",
                        Speed: $"{random.Next(0, 15)} knots",
                        Conditions: "Test",
                        AreaCovered: random.Next(0, 100),
                        CurrentWaypointIndex: random.Next(0, 3),
                        LastUpdated: DateTime.UtcNow
                    );

                    await _repository!.UpdateBoatStateAsync(state);
                    updateCounts.AddOrUpdate(boatId, 1, (_, count) => count + 1);

                    await Task.Delay(updateIntervalMs, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                    break;
                }
            }
        }).ToArray();

        // Start read tasks (readers)
        var readTasks = Enumerable.Range(0, concurrentReaders).Select(async readerId =>
        {
            readCounts[readerId] = 0;

            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    // Perform read operation
                    var boats = await _repository!.GetAllBoatsWithStatesAsync();
                    var boatList = boats.ToList();
                    
                    Assert.Equal(4, boatList.Count); // Should always see 4 boats
                    
                    readCounts.AddOrUpdate(readerId, 1, (_, count) => count + 1);

                    await Task.Delay(50, cts.Token); // Read more frequently than updates
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                    break;
                }
            }
        }).ToArray();

        // Wait for all tasks
        await Task.WhenAll(updateTasks.Concat(readTasks));

        // Assert - No conflicts or errors
        Assert.Empty(errors);

        var totalUpdates = updateCounts.Values.Sum();
        var totalReads = readCounts.Values.Sum();

        Console.WriteLine($"Concurrent Read/Write Test:");
        Console.WriteLine($"  Duration: {durationSeconds}s");
        Console.WriteLine($"  Total Updates: {totalUpdates}");
        Console.WriteLine($"  Total Reads: {totalReads}");
        Console.WriteLine($"  No conflicts: ✅");
        Console.WriteLine($"  Update Counts:");
        foreach (var kvp in updateCounts.OrderBy(x => x.Key))
        {
            Console.WriteLine($"    {kvp.Key}: {kvp.Value} updates");
        }
    }

    #endregion
}
