using Npgsql;

namespace EnergyBoatApp.ApiService.Services;

/// <summary>
/// Background service that seeds initial boat data if the database is empty.
/// Populates boats, boat_states, routes, and waypoints tables with 4 boats.
/// Safe to run multiple times (checks if data already exists).
/// </summary>
public class SeedDataService : IHostedService
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<SeedDataService> _logger;

    public SeedDataService(
        NpgsqlDataSource dataSource,
        ILogger<SeedDataService> logger)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking if database needs seeding...");

        try
        {
            // Check if boats table is empty
            if (await HasDataAsync(cancellationToken))
            {
                _logger.LogInformation("Database already contains data, skipping seed");
                return;
            }

            // Seed the initial data
            await SeedInitialDataAsync(cancellationToken);

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database seeding failed");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Seed data service stopping");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks if the boats table already has data.
    /// </summary>
    private async Task<bool> HasDataAsync(CancellationToken cancellationToken)
    {
        const string checkDataQuery = "SELECT COUNT(*) FROM boats";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = checkDataQuery;

        var count = (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 0L);

        _logger.LogDebug("Found {BoatCount} boats in database", count);

        return count > 0;
    }

    /// <summary>
    /// Seeds all initial data for 4 boats including their states, routes, and waypoints.
    /// Data from data-model.md Sample Data sections.
    /// </summary>
    private async Task SeedInitialDataAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Seeding initial data for 4 boats...");

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Seed boats (static metadata)
            await SeedBoatsAsync(connection, transaction, cancellationToken);

            // Seed boat states (initial runtime state)
            await SeedBoatStatesAsync(connection, transaction, cancellationToken);

            // Seed routes (route metadata)
            await SeedRoutesAsync(connection, transaction, cancellationToken);

            // Seed waypoints (geographic points)
            await SeedWaypointsAsync(connection, transaction, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Successfully seeded 4 boats with routes and waypoints");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed data, rolling back transaction");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task SeedBoatsAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        const string insertBoats = @"
            INSERT INTO boats (id, vessel_name, crew_count, equipment, project, survey_type)
            VALUES 
                ('BOAT-001', 'Contoso Sea Voyager', 24, 'Multibeam Sonar, Magnetometer', 'Dogger Bank Offshore Wind Farm', 'Geophysical Survey'),
                ('BOAT-002', 'Contoso Sea Pioneer', 18, 'ROV, Side-scan Sonar', 'Subsea Cable Route Survey', 'ROV Operations'),
                ('BOAT-003', 'Contoso Sea Navigator', 22, 'CPT, Seabed Sampling', 'North Sea Pipeline Inspection', 'Geotechnical Survey'),
                ('BOAT-004', 'Contoso Sea Explorer', 12, 'Multibeam, Sub-bottom Profiler', 'Scheduled Maintenance', 'Standby')";

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = insertBoats;
        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Seeded 4 boats");
    }

    private async Task SeedBoatStatesAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        const string insertBoatStates = @"
            INSERT INTO boat_states (
                boat_id, latitude, longitude, heading, speed_knots, original_speed_knots, 
                energy_level, status, speed, conditions, area_covered, current_waypoint_index
            )
            VALUES 
                ('BOAT-001', 51.5074, -0.1278, 45.0, 12.0, 12.0, 85.5, 'Active', '12 knots', 'Good sea state', 0.0, 0),
                ('BOAT-002', 51.5154, -0.1420, 0.0, 0.0, 0.0, 42.3, 'Charging', 'Station keeping', 'Calm seas', 0.0, 0),
                ('BOAT-003', 51.5010, -0.1200, 135.0, 8.0, 8.0, 91.2, 'Active', '8 knots', 'Moderate seas, 2m swell', 0.0, 0),
                ('BOAT-004', 51.5090, -0.1390, 315.0, 0.0, 0.0, 15.7, 'Maintenance', 'Docked', 'At berth', 0.0, 0)";

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = insertBoatStates;
        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Seeded 4 boat states");
    }

    private async Task SeedRoutesAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        const string insertRoutes = @"
            INSERT INTO routes (boat_id, route_name)
            VALUES 
                ('BOAT-001', 'Rectangle Pattern - NE Quadrant'),
                ('BOAT-002', 'Zigzag Pattern - NW Quadrant'),
                ('BOAT-003', 'Triangle Pattern - South Quadrant'),
                ('BOAT-004', 'Docked Position (Maintenance)')";

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = insertRoutes;
        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Seeded 4 routes");
    }

    private async Task SeedWaypointsAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        // BOAT-001: Rectangle Pattern (5 waypoints - returns to start)
        const string insertWaypoints = @"
            INSERT INTO waypoints (boat_id, latitude, longitude, sequence)
            VALUES 
                -- BOAT-001: Rectangle Pattern - NE Quadrant
                ('BOAT-001', 51.5170, -0.1278, 0),
                ('BOAT-001', 51.5250, -0.1000, 1),
                ('BOAT-001', 51.5250, -0.1400, 2),
                ('BOAT-001', 51.5170, -0.1400, 3),
                ('BOAT-001', 51.5170, -0.1278, 4),
                
                -- BOAT-002: Zigzag Pattern - NW Quadrant (5 waypoints)
                ('BOAT-002', 51.5200, -0.1500, 0),
                ('BOAT-002', 51.5250, -0.1600, 1),
                ('BOAT-002', 51.5200, -0.1700, 2),
                ('BOAT-002', 51.5150, -0.1600, 3),
                ('BOAT-002', 51.5200, -0.1500, 4),
                
                -- BOAT-003: Triangle Pattern - South Quadrant (4 waypoints)
                ('BOAT-003', 51.4950, -0.1200, 0),
                ('BOAT-003', 51.5050, -0.1100, 1),
                ('BOAT-003', 51.5050, -0.1300, 2),
                ('BOAT-003', 51.4950, -0.1200, 3),
                
                -- BOAT-004: Docked Position (1 waypoint - maintenance)
                ('BOAT-004', 51.5090, -0.1390, 0)";

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = insertWaypoints;
        var waypointCount = await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Seeded {WaypointCount} waypoints across 4 boats", waypointCount);
    }
}
