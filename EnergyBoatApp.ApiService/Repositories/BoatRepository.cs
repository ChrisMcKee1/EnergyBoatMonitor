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

        _logger.LogDebug("Retrieved {Count} boats with states", results.Count);
        return results;
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

        if (rowsAffected == 0)
        {
            _logger.LogWarning("Boat state update affected 0 rows for {BoatId}", state.BoatId);
        }
        else
        {
            _logger.LogDebug("Updated boat state for {BoatId}", state.BoatId);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Models.Waypoint>> GetWaypointsForBoatAsync(string boatId)
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

        _logger.LogDebug("Retrieved {Count} waypoints for boat {BoatId}", waypoints.Count, boatId);
        return waypoints;
    }

    /// <inheritdoc/>
    public async Task<int> ResetAllBoatsAsync()
    {
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

            _logger.LogInformation("Reset {Count} boats to initial state", rowsAffected);
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset boats, rolling back transaction");
            await transaction.RollbackAsync();
            throw;
        }
    }
}
