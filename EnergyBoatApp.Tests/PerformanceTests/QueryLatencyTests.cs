using System.Diagnostics;
using EnergyBoatApp.ApiService.Models;
using EnergyBoatApp.ApiService.Repositories;
using Microsoft.Extensions.Logging;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EnergyBoatApp.Tests.PerformanceTests;

/// <summary>
/// Performance tests for query latency benchmarking.
/// Validates that GET /api/boats queries meet <200ms p95 latency requirement under load.
/// Tests simulate 10x simulation speed (30 req/min baseline, 300 req/min at 10x).
/// </summary>
public class QueryLatencyTests : IAsyncLifetime
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

        // Create data source with connection pooling
        var connectionString = _postgresContainer.GetConnectionString();
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
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

            -- Performance indexes (matching production)
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

        // Add waypoints (3 per boat = 12 total)
        const string insertWaypoints = @"
            INSERT INTO waypoints (boat_id, latitude, longitude, sequence, created_at) VALUES
            -- BOAT-001 waypoints
            ('BOAT-001', 51.5074, -0.1278, 0, NOW()),
            ('BOAT-001', 51.5200, -0.1100, 1, NOW()),
            ('BOAT-001', 51.5300, -0.0950, 2, NOW()),
            -- BOAT-002 waypoints
            ('BOAT-002', 51.5154, -0.1420, 0, NOW()),
            ('BOAT-002', 51.5050, -0.1600, 1, NOW()),
            ('BOAT-002', 51.4950, -0.1500, 2, NOW()),
            -- BOAT-003 waypoints
            ('BOAT-003', 51.5010, -0.0920, 0, NOW()),
            ('BOAT-003', 51.4900, -0.1000, 1, NOW()),
            ('BOAT-003', 51.4800, -0.1100, 2, NOW()),
            -- BOAT-004 waypoints
            ('BOAT-004', 51.4885, -0.1650, 0, NOW()),
            ('BOAT-004', 51.5000, -0.1750, 1, NOW()),
            ('BOAT-004', 51.5100, -0.1850, 2, NOW())";

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

        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = insertWaypoints;
            await cmd.ExecuteNonQueryAsync();
        }
    }

    #region Baseline Performance Tests

    [Fact]
    public async Task GetAllBoatsWithStatesAsync_MeetsP50LatencyTarget()
    {
        // Target: p50 < 25ms (data-model.md specifies <50ms for all boats query)
        // This is the "happy path" - 50th percentile should be very fast

        const int iterations = 100;
        var latencies = new List<long>(iterations);

        // Warmup (exclude from measurements)
        await _repository!.GetAllBoatsWithStatesAsync();

        // Measure
        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var result = await _repository.GetAllBoatsWithStatesAsync();
            sw.Stop();

            latencies.Add(sw.ElapsedMilliseconds);
            
            // Verify data integrity
            Assert.NotEmpty(result);
        }

        // Calculate p50 (median)
        latencies.Sort();
        var p50 = latencies[iterations / 2];

        // Assert
        Assert.True(p50 < 25, $"p50 latency {p50}ms exceeds 25ms target (target is <50ms, aiming for <25ms)");
        
        // Report statistics
        var p95 = latencies[(int)(iterations * 0.95)];
        var p99 = latencies[(int)(iterations * 0.99)];
        var avg = latencies.Average();
        
        Console.WriteLine($"GetAllBoatsWithStatesAsync Latency Statistics:");
        Console.WriteLine($"  Iterations: {iterations}");
        Console.WriteLine($"  Average:    {avg:F2}ms");
        Console.WriteLine($"  p50:        {p50}ms (target: <25ms)");
        Console.WriteLine($"  p95:        {p95}ms");
        Console.WriteLine($"  p99:        {p99}ms");
    }

    [Fact]
    public async Task GetAllBoatsWithStatesAsync_MeetsP95LatencyTarget()
    {
        // Primary requirement: p95 < 200ms (plan.md Performance Goals)
        // This is the critical SLA metric for the API

        const int iterations = 100;
        var latencies = new List<long>(iterations);

        // Warmup
        await _repository!.GetAllBoatsWithStatesAsync();

        // Measure
        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var result = await _repository.GetAllBoatsWithStatesAsync();
            sw.Stop();

            latencies.Add(sw.ElapsedMilliseconds);
        }

        // Calculate p95
        latencies.Sort();
        var p95 = latencies[(int)(iterations * 0.95)];

        // Assert - PRIMARY SLA REQUIREMENT
        Assert.True(p95 < 200, $"p95 latency {p95}ms exceeds 200ms SLA target (CRITICAL)");
        
        // Report full statistics
        var p50 = latencies[iterations / 2];
        var p99 = latencies[(int)(iterations * 0.99)];
        var avg = latencies.Average();
        var max = latencies.Max();
        
        Console.WriteLine($"GetAllBoatsWithStatesAsync p95 Latency Test:");
        Console.WriteLine($"  Iterations: {iterations}");
        Console.WriteLine($"  Average:    {avg:F2}ms");
        Console.WriteLine($"  p50:        {p50}ms");
        Console.WriteLine($"  p95:        {p95}ms (SLA target: <200ms)");
        Console.WriteLine($"  p99:        {p99}ms");
        Console.WriteLine($"  Max:        {max}ms");
    }

    [Fact]
    public async Task GetAllBoatsWithStatesAsync_UnderSimulated10xLoad()
    {
        // Simulate 10x speed: 30 req/min baseline → 300 req/min at 10x
        // Over 60 seconds = 300 requests total
        // Test with 300 concurrent requests to simulate peak load

        const int totalRequests = 300;
        const int concurrencyLevel = 10; // 10 concurrent requests at a time
        var latencies = new List<long>(totalRequests);
        var latencyLock = new object();

        // Warmup
        await _repository!.GetAllBoatsWithStatesAsync();

        // Measure under concurrent load
        var batches = Enumerable.Range(0, totalRequests / concurrencyLevel);
        
        var sw = Stopwatch.StartNew();
        
        foreach (var batch in batches)
        {
            var tasks = Enumerable.Range(0, concurrencyLevel).Select(async _ =>
            {
                var querySw = Stopwatch.StartNew();
                var result = await _repository.GetAllBoatsWithStatesAsync();
                querySw.Stop();

                lock (latencyLock)
                {
                    latencies.Add(querySw.ElapsedMilliseconds);
                }

                return result;
            });

            await Task.WhenAll(tasks);
        }
        
        sw.Stop();

        // Calculate statistics
        latencies.Sort();
        var p50 = latencies[latencies.Count / 2];
        var p95 = latencies[(int)(latencies.Count * 0.95)];
        var p99 = latencies[(int)(latencies.Count * 0.99)];
        var avg = latencies.Average();
        var max = latencies.Max();
        var totalTime = sw.ElapsedMilliseconds;
        var throughput = (totalRequests * 1000.0) / totalTime; // req/sec

        // Assert - p95 must still meet SLA under load
        Assert.True(p95 < 200, $"p95 latency {p95}ms exceeds 200ms under 10x load (CRITICAL)");
        
        // Report
        Console.WriteLine($"10x Load Simulation (300 requests, {concurrencyLevel} concurrent):");
        Console.WriteLine($"  Total Time:  {totalTime}ms");
        Console.WriteLine($"  Throughput:  {throughput:F2} req/sec");
        Console.WriteLine($"  Average:     {avg:F2}ms");
        Console.WriteLine($"  p50:         {p50}ms");
        Console.WriteLine($"  p95:         {p95}ms (SLA: <200ms)");
        Console.WriteLine($"  p99:         {p99}ms");
        Console.WriteLine($"  Max:         {max}ms");
    }

    #endregion

    #region Individual Operation Performance Tests

    [Fact]
    public async Task GetBoatByIdAsync_MeetsLatencyTarget()
    {
        // Target: <20ms average (single row lookup should be very fast)

        const int iterations = 100;
        var latencies = new List<long>(iterations);

        // Warmup
        await _repository!.GetBoatByIdAsync("BOAT-001");

        // Measure
        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var result = await _repository.GetBoatByIdAsync("BOAT-001");
            sw.Stop();

            latencies.Add(sw.ElapsedMilliseconds);
            Assert.NotNull(result);
        }

        // Statistics
        latencies.Sort();
        var avg = latencies.Average();
        var p95 = latencies[(int)(iterations * 0.95)];

        Assert.True(avg < 20, $"Average latency {avg:F2}ms exceeds 20ms target for single boat lookup");
        
        Console.WriteLine($"GetBoatByIdAsync Latency:");
        Console.WriteLine($"  Average: {avg:F2}ms (target: <20ms)");
        Console.WriteLine($"  p95:     {p95}ms");
    }

    [Fact]
    public async Task UpdateBoatStateAsync_MeetsLatencyTarget()
    {
        // Target: <10ms (data-model.md specification for state updates)

        const int iterations = 100;
        var latencies = new List<long>(iterations);

        var testState = new BoatState(
            BoatId: "BOAT-001",
            Latitude: 51.5074,
            Longitude: -0.1278,
            Heading: 45.0,
            SpeedKnots: 12.0,
            OriginalSpeedKnots: 12.0,
            EnergyLevel: 85.5,
            Status: "Active",
            Speed: "12 knots",
            Conditions: "Good",
            AreaCovered: 23.7,
            CurrentWaypointIndex: 0,
            LastUpdated: DateTime.UtcNow
        );

        // Warmup
        await _repository!.UpdateBoatStateAsync(testState);

        // Measure
        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            await _repository.UpdateBoatStateAsync(testState);
            sw.Stop();

            latencies.Add(sw.ElapsedMilliseconds);
        }

        // Statistics
        latencies.Sort();
        var avg = latencies.Average();
        var p95 = latencies[(int)(iterations * 0.95)];

        Assert.True(avg < 10, $"Average latency {avg:F2}ms exceeds 10ms target for state update");
        
        Console.WriteLine($"UpdateBoatStateAsync Latency:");
        Console.WriteLine($"  Average: {avg:F2}ms (target: <10ms)");
        Console.WriteLine($"  p95:     {p95}ms");
    }

    [Fact]
    public async Task GetWaypointsForBoatAsync_MeetsLatencyTarget()
    {
        // Target: <20ms (data-model.md specification for waypoint queries)

        const int iterations = 100;
        var latencies = new List<long>(iterations);

        // Warmup
        await _repository!.GetWaypointsForBoatAsync("BOAT-001");

        // Measure
        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var result = await _repository.GetWaypointsForBoatAsync("BOAT-001");
            sw.Stop();

            latencies.Add(sw.ElapsedMilliseconds);
            Assert.NotEmpty(result);
        }

        // Statistics
        latencies.Sort();
        var avg = latencies.Average();
        var p95 = latencies[(int)(iterations * 0.95)];

        Assert.True(avg < 20, $"Average latency {avg:F2}ms exceeds 20ms target for waypoint query");
        
        Console.WriteLine($"GetWaypointsForBoatAsync Latency:");
        Console.WriteLine($"  Average: {avg:F2}ms (target: <20ms)");
        Console.WriteLine($"  p95:     {p95}ms");
    }

    #endregion

    #region Query Plan Analysis

    [Fact]
    public async Task GetAllBoatsWithStatesAsync_UsesIndexes()
    {
        // Verify query plan uses indexes and avoids full table scans
        // This test helps identify slow queries for EXPLAIN ANALYZE optimization

        const string explainQuery = @"
            EXPLAIN (ANALYZE, BUFFERS, FORMAT JSON)
            SELECT 
                b.id, b.vessel_name, b.crew_count, b.equipment, b.project, b.survey_type, b.created_at, b.updated_at,
                s.boat_id, s.latitude, s.longitude, s.heading, s.speed_knots, s.original_speed_knots,
                s.energy_level, s.status, s.speed, s.conditions, s.area_covered, s.current_waypoint_index, s.last_updated
            FROM boats b
            INNER JOIN boat_states s ON b.id = s.boat_id";

        await using var connection = await _dataSource!.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = explainQuery;

        var result = await command.ExecuteScalarAsync() as string;
        Assert.NotNull(result);

        // Parse EXPLAIN output (basic validation)
        Assert.Contains("Hash Join", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Seq Scan on boat_states", result); // Should use primary key index
        
        Console.WriteLine("Query Plan for GetAllBoatsWithStatesAsync:");
        Console.WriteLine(result);
    }

    #endregion
}
