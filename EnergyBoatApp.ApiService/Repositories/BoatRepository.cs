using Npgsql;
using EnergyBoatApp.ApiService.Models;

namespace EnergyBoatApp.ApiService.Repositories;

/// <summary>
/// PostgreSQL-backed repository for boat data access.
/// Uses NpgsqlDataSource for connection pooling, async/await patterns, and direct SQL queries.
/// </summary>
public class BoatRepository : IBoatRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<BoatRepository> _logger;

    public BoatRepository(NpgsqlDataSource dataSource, ILogger<BoatRepository> logger)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<(Boat boat, BoatState state)>> GetAllBoatsWithStatesAsync()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogDebug("GetAllBoatsWithStatesAsync: Starting query");

        const string sql = @"
            SELECT 
                b.id, b.vessel_name, b.crew_count, b.equipment, b.project, b.survey_type, 
                b.created_at, b.updated_at,
                bs.boat_id, bs.latitude, bs.longitude, bs.heading, bs.speed_knots, 
                bs.original_speed_knots, bs.energy_level, bs.status, bs.speed, 
                bs.conditions, bs.area_covered, bs.current_waypoint_index, bs.last_updated
            FROM boats b
            INNER JOIN boat_states bs ON b.id = bs.boat_id
            ORDER BY b.id";

        var results = new List<(Boat, BoatState)>();

        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
        {
            var boat = new Boat(
                Id: reader.GetString(0),
                VesselName: reader.GetString(1),
                CrewCount: reader.GetInt32(2),
                Equipment: reader.GetString(3),
                Project: reader.GetString(4),
                SurveyType: reader.GetString(5),
                CreatedAt: reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                UpdatedAt: reader.IsDBNull(7) ? null : reader.GetDateTime(7)
            );

            var state = new BoatState(
                BoatId: reader.GetString(8),
                Latitude: reader.GetDouble(9),
                Longitude: reader.GetDouble(10),
                Heading: reader.GetDouble(11),
                SpeedKnots: reader.GetDouble(12),
                OriginalSpeedKnots: reader.GetDouble(13),
                EnergyLevel: reader.GetDouble(14),
                Status: reader.GetString(15),
                Speed: reader.GetString(16),
                Conditions: reader.GetString(17),
                AreaCovered: reader.GetDouble(18),
                CurrentWaypointIndex: reader.GetInt32(19),
                LastUpdated: reader.IsDBNull(20) ? null : reader.GetDateTime(20)
            );

            results.Add((boat, state));
        }

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (elapsedMs > 100)
            {
                _logger.LogWarning("GetAllBoatsWithStatesAsync: Slow query detected - {ElapsedMs}ms (threshold: 100ms), returned {Count} boats", 
                    elapsedMs, results.Count);
            }
            else
            {
                _logger.LogDebug("GetAllBoatsWithStatesAsync: Query completed in {ElapsedMs}ms, returned {Count} boats", 
                    elapsedMs, results.Count);
            }

            return results;
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "GetAllBoatsWithStatesAsync: Database error occurred - {ErrorCode}", ex.ErrorCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllBoatsWithStatesAsync: Unexpected error occurred");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<(Boat boat, BoatState state)?> GetBoatByIdAsync(string id)
    {
        const string sql = @"
            SELECT 
                b.id, b.vessel_name, b.crew_count, b.equipment, b.project, b.survey_type, 
                b.created_at, b.updated_at,
                bs.boat_id, bs.latitude, bs.longitude, bs.heading, bs.speed_knots, 
                bs.original_speed_knots, bs.energy_level, bs.status, bs.speed, 
                bs.conditions, bs.area_covered, bs.current_waypoint_index, bs.last_updated
            FROM boats b
            INNER JOIN boat_states bs ON b.id = bs.boat_id
            WHERE b.id = @id";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            _logger.LogWarning("Boat {BoatId} not found", id);
            return null;
        }

        var boat = new Boat(
            Id: reader.GetString(0),
            VesselName: reader.GetString(1),
            CrewCount: reader.GetInt32(2),
            Equipment: reader.GetString(3),
            Project: reader.GetString(4),
            SurveyType: reader.GetString(5),
            CreatedAt: reader.IsDBNull(6) ? null : reader.GetDateTime(6),
            UpdatedAt: reader.IsDBNull(7) ? null : reader.GetDateTime(7)
        );

        var state = new BoatState(
            BoatId: reader.GetString(8),
            Latitude: reader.GetDouble(9),
            Longitude: reader.GetDouble(10),
            Heading: reader.GetDouble(11),
            SpeedKnots: reader.GetDouble(12),
            OriginalSpeedKnots: reader.GetDouble(13),
            EnergyLevel: reader.GetDouble(14),
            Status: reader.GetString(15),
            Speed: reader.GetString(16),
            Conditions: reader.GetString(17),
            AreaCovered: reader.GetDouble(18),
            CurrentWaypointIndex: reader.GetInt32(19),
            LastUpdated: reader.IsDBNull(20) ? null : reader.GetDateTime(20)
        );

        return (boat, state);
    }

    /// <inheritdoc/>
    public async Task UpdateBoatStateAsync(BoatState state)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogDebug("UpdateBoatStateAsync: Starting update for {BoatId}, Status={Status}, EnergyLevel={EnergyLevel}", 
            state.BoatId, state.Status, state.EnergyLevel);
        
        try
        {
            const string sql = @"
                UPDATE boat_states
                SET latitude = @latitude,
                    longitude = @longitude,
                    heading = @heading,
                    speed_knots = @speedKnots,
                    original_speed_knots = @originalSpeedKnots,
                    energy_level = @energyLevel,
                    status = @status,
                    speed = @speed,
                    conditions = @conditions,
                    area_covered = @areaCovered,
                    current_waypoint_index = @currentWaypointIndex,
                    last_updated = @lastUpdated
                WHERE boat_id = @boatId";

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            command.Parameters.AddWithValue("@latitude", state.Latitude);
            command.Parameters.AddWithValue("@longitude", state.Longitude);
            command.Parameters.AddWithValue("@heading", state.Heading);
            command.Parameters.AddWithValue("@speedKnots", state.SpeedKnots);
            command.Parameters.AddWithValue("@originalSpeedKnots", state.OriginalSpeedKnots);
            command.Parameters.AddWithValue("@energyLevel", state.EnergyLevel);
            command.Parameters.AddWithValue("@status", state.Status);
            command.Parameters.AddWithValue("@speed", state.Speed);
            command.Parameters.AddWithValue("@conditions", state.Conditions);
            command.Parameters.AddWithValue("@areaCovered", state.AreaCovered);
            command.Parameters.AddWithValue("@currentWaypointIndex", state.CurrentWaypointIndex);
            command.Parameters.AddWithValue("@lastUpdated", state.LastUpdated ?? DateTime.UtcNow);
            command.Parameters.AddWithValue("@boatId", state.BoatId);

            var rowsAffected = await command.ExecuteNonQueryAsync();

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (rowsAffected == 0)
            {
                _logger.LogWarning("UpdateBoatStateAsync: Update affected 0 rows for {BoatId} in {ElapsedMs}ms - boat may not exist", 
                    state.BoatId, elapsedMs);
            }
            else if (elapsedMs > 100)
            {
                _logger.LogWarning("UpdateBoatStateAsync: Slow query detected - {ElapsedMs}ms (threshold: 100ms), updated {RowsAffected} rows for {BoatId}", 
                    elapsedMs, rowsAffected, state.BoatId);
            }
            else
            {
                _logger.LogDebug("UpdateBoatStateAsync: Completed in {ElapsedMs}ms, updated {RowsAffected} rows for {BoatId}", 
                    elapsedMs, rowsAffected, state.BoatId);
            }
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "UpdateBoatStateAsync: Database error occurred for {BoatId} - {ErrorCode}", 
                state.BoatId, ex.ErrorCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateBoatStateAsync: Unexpected error occurred for {BoatId}", state.BoatId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Models.Waypoint>> GetWaypointsForBoatAsync(string boatId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogDebug("GetWaypointsForBoatAsync: Starting query for {BoatId}", boatId);
        
        try
        {
            const string sql = @"
                SELECT id, boat_id, latitude, longitude, sequence, created_at
                FROM waypoints
                WHERE boat_id = @boatId
                ORDER BY sequence";

            var waypoints = new List<Models.Waypoint>();

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@boatId", boatId);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var waypoint = new Models.Waypoint(
                    Id: reader.GetInt32(0),
                    BoatId: reader.GetString(1),
                    Latitude: reader.GetDouble(2),
                    Longitude: reader.GetDouble(3),
                    Sequence: reader.GetInt32(4),
                    CreatedAt: reader.IsDBNull(5) ? null : reader.GetDateTime(5)
                );

                waypoints.Add(waypoint);
            }

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (elapsedMs > 100)
            {
                _logger.LogWarning("GetWaypointsForBoatAsync: Slow query detected - {ElapsedMs}ms (threshold: 100ms), returned {Count} waypoints for {BoatId}", 
                    elapsedMs, waypoints.Count, boatId);
            }
            else
            {
                _logger.LogDebug("GetWaypointsForBoatAsync: Completed in {ElapsedMs}ms, returned {Count} waypoints for {BoatId}", 
                    elapsedMs, waypoints.Count, boatId);
            }

            return waypoints;
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "GetWaypointsForBoatAsync: Database error occurred for {BoatId} - {ErrorCode}", 
                boatId, ex.ErrorCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetWaypointsForBoatAsync: Unexpected error occurred for {BoatId}", boatId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> ResetAllBoatsAsync()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogDebug("ResetAllBoatsAsync: Starting boat state reset transaction");
        
        // Initial state values from data-model.md sample data
        const string sql = @"
            UPDATE boat_states
            SET 
                latitude = CASE boat_id
                    WHEN 'BOAT-001' THEN 51.5074
                    WHEN 'BOAT-002' THEN 51.5154
                    WHEN 'BOAT-003' THEN 51.5010
                    WHEN 'BOAT-004' THEN 51.5090
                END,
                longitude = CASE boat_id
                    WHEN 'BOAT-001' THEN -0.1278
                    WHEN 'BOAT-002' THEN -0.1420
                    WHEN 'BOAT-003' THEN -0.1200
                    WHEN 'BOAT-004' THEN -0.1390
                END,
                heading = CASE boat_id
                    WHEN 'BOAT-001' THEN 45.0
                    WHEN 'BOAT-002' THEN 0.0
                    WHEN 'BOAT-003' THEN 135.0
                    WHEN 'BOAT-004' THEN 315.0
                END,
                speed_knots = CASE boat_id
                    WHEN 'BOAT-001' THEN 12.0
                    WHEN 'BOAT-002' THEN 0.0
                    WHEN 'BOAT-003' THEN 8.0
                    WHEN 'BOAT-004' THEN 0.0
                END,
                original_speed_knots = CASE boat_id
                    WHEN 'BOAT-001' THEN 12.0
                    WHEN 'BOAT-002' THEN 0.0
                    WHEN 'BOAT-003' THEN 8.0
                    WHEN 'BOAT-004' THEN 0.0
                END,
                energy_level = CASE boat_id
                    WHEN 'BOAT-001' THEN 85.5
                    WHEN 'BOAT-002' THEN 42.3
                    WHEN 'BOAT-003' THEN 91.2
                    WHEN 'BOAT-004' THEN 15.7
                END,
                status = CASE boat_id
                    WHEN 'BOAT-001' THEN 'Active'
                    WHEN 'BOAT-002' THEN 'Charging'
                    WHEN 'BOAT-003' THEN 'Active'
                    WHEN 'BOAT-004' THEN 'Maintenance'
                END,
                current_waypoint_index = 0,
                last_updated = @lastUpdated";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = sql;
            command.Parameters.AddWithValue("@lastUpdated", DateTime.UtcNow);

            var rowsAffected = await command.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (rowsAffected == 0)
            {
                _logger.LogWarning("ResetAllBoatsAsync: Reset affected 0 rows in {ElapsedMs}ms - no boat states updated", elapsedMs);
            }
            else if (elapsedMs > 100)
            {
                _logger.LogWarning("ResetAllBoatsAsync: Slow query detected - {ElapsedMs}ms (threshold: 100ms), reset {Count} boats", 
                    elapsedMs, rowsAffected);
            }
            else
            {
                _logger.LogInformation("ResetAllBoatsAsync: Successfully reset {Count} boats to initial state in {ElapsedMs}ms", 
                    rowsAffected, elapsedMs);
            }
            
            return rowsAffected;
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "ResetAllBoatsAsync: Database error occurred during reset, rolling back transaction - {ErrorCode}", 
                ex.ErrorCode);
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ResetAllBoatsAsync: Unexpected error occurred during reset, rolling back transaction");
            await transaction.RollbackAsync();
            throw;
        }
    }
}
