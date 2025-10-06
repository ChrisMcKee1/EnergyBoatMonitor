namespace EnergyBoatApp.ApiService.Models;

/// <summary>
/// Represents dynamic runtime state for a boat stored in the boat_states table.
/// This data is updated on every simulation tick (every 2 seconds at 1x speed).
/// </summary>
/// <param name="BoatId">Foreign key reference to boats.id</param>
/// <param name="Latitude">Current latitude position (-90 to 90 degrees)</param>
/// <param name="Longitude">Current longitude position (-180 to 180 degrees)</param>
/// <param name="Heading">Current heading in degrees (0-360, where 0=North)</param>
/// <param name="SpeedKnots">Current speed in knots (>=0)</param>
/// <param name="OriginalSpeedKnots">Original speed before status changes (for reset logic)</param>
/// <param name="EnergyLevel">Battery level percentage (0-100)</param>
/// <param name="Status">Operational status: Active, Charging, or Maintenance</param>
/// <param name="Speed">Human-readable speed description (e.g., "12 knots", "Station keeping")</param>
/// <param name="Conditions">Environmental conditions (e.g., "Moderate seas, 2m swell")</param>
/// <param name="AreaCovered">Total area surveyed in square kilometers</param>
/// <param name="CurrentWaypointIndex">Index of current target waypoint in route (>=0)</param>
/// <param name="LastUpdated">Timestamp of last state update</param>
public record BoatState(
    string BoatId,
    double Latitude,
    double Longitude,
    double Heading,
    double SpeedKnots,
    double OriginalSpeedKnots,
    double EnergyLevel,
    string Status,
    string Speed,
    string Conditions,
    double AreaCovered,
    int CurrentWaypointIndex,
    DateTime? LastUpdated = null
);
