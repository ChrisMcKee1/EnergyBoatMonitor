namespace EnergyBoatApp.ApiService.Models;

/// <summary>
/// Represents a geographic waypoint in a boat's survey route stored in the waypoints table.
/// Boats navigate through waypoints in sequence order, looping back to 0 after the last waypoint.
/// </summary>
/// <param name="Id">Unique waypoint identifier (database auto-generated)</param>
/// <param name="BoatId">Foreign key reference to boats.id</param>
/// <param name="Latitude">Waypoint latitude (-90 to 90 degrees)</param>
/// <param name="Longitude">Waypoint longitude (-180 to 180 degrees)</param>
/// <param name="Sequence">Order index for this waypoint in the route (>=0, unique per boat_id)</param>
/// <param name="CreatedAt">Timestamp when waypoint was created</param>
public record Waypoint(
    int Id,
    string BoatId,
    double Latitude,
    double Longitude,
    int Sequence,
    DateTime? CreatedAt = null
);
