namespace EnergyBoatApp.ApiService.Models;

/// <summary>
/// Represents survey route metadata stored in the routes table.
/// Each boat has exactly one route (1:1 relationship with boats table).
/// </summary>
/// <param name="BoatId">Foreign key reference to boats.id (also serves as primary key)</param>
/// <param name="RouteName">Descriptive name of the route (e.g., "Rectangle Pattern - NE Quadrant")</param>
/// <param name="CreatedAt">Timestamp when route was created</param>
public record Route(
    string BoatId,
    string RouteName,
    DateTime? CreatedAt = null
);
