using EnergyBoatApp.ApiService.Models;

namespace EnergyBoatApp.ApiService.Repositories;

/// <summary>
/// Repository interface for boat data access operations.
/// Provides async methods for querying and updating boat state, waypoints, and reset functionality.
/// All methods are designed for NpgsqlDataSource connection pooling and async/await patterns.
/// </summary>
public interface IBoatRepository
{
    /// <summary>
    /// Retrieves all boats with their current runtime states.
    /// Joins boats table with boat_states table.
    /// Target latency: &lt;50ms (called every 2 seconds by frontend).
    /// </summary>
    /// <returns>Collection of boats with their current states</returns>
    Task<IEnumerable<(Boat boat, BoatState state)>> GetAllBoatsWithStatesAsync();

    /// <summary>
    /// Retrieves a single boat by ID with its current state.
    /// </summary>
    /// <param name="id">Boat identifier (e.g., "BOAT-001")</param>
    /// <returns>Boat and state tuple, or null if not found</returns>
    Task<(Boat boat, BoatState state)?> GetBoatByIdAsync(string id);

    /// <summary>
    /// Updates a boat's runtime state in the boat_states table.
    /// Called on every simulation tick (every 2 seconds at 1x speed, every 200ms at 10x speed).
    /// Target latency: &lt;10ms per update.
    /// </summary>
    /// <param name="state">Updated boat state with new position, heading, energy, etc.</param>
    Task UpdateBoatStateAsync(BoatState state);

    /// <summary>
    /// Retrieves all waypoints for a specific boat's route, ordered by sequence.
    /// Target latency: &lt;20ms (called on status transitions and route initialization).
    /// </summary>
    /// <param name="boatId">Boat identifier</param>
    /// <returns>Ordered collection of waypoints for the boat's route</returns>
    Task<IEnumerable<Models.Waypoint>> GetWaypointsForBoatAsync(string boatId);

    /// <summary>
    /// Resets all boats to their initial states (position, heading, energy, status, waypoint index).
    /// Called by POST /api/boats/reset endpoint.
    /// Uses transaction for atomicity - all boats reset or none.
    /// </summary>
    /// <returns>Number of boats reset (should be 4 in production)</returns>
    Task<int> ResetAllBoatsAsync();
}
