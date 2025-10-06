namespace EnergyBoatApp.ApiService.Models;

/// <summary>
/// Represents static vessel metadata stored in the boats table.
/// This data is immutable after seeding (vessel name, equipment, crew, project info).
/// </summary>
/// <param name="Id">Unique boat identifier (e.g., "BOAT-001")</param>
/// <param name="VesselName">Name of the survey vessel (e.g., "Contoso Sea Voyager")</param>
/// <param name="CrewCount">Number of crew members on board</param>
/// <param name="Equipment">Survey equipment installed (e.g., "Multibeam Sonar, Magnetometer")</param>
/// <param name="Project">Current project assignment (e.g., "Dogger Bank Offshore Wind Farm")</param>
/// <param name="SurveyType">Type of survey operation (e.g., "Geophysical Survey")</param>
/// <param name="CreatedAt">Timestamp when boat record was created</param>
/// <param name="UpdatedAt">Timestamp of last metadata update</param>
public record Boat(
    string Id,
    string VesselName,
    int CrewCount,
    string Equipment,
    string Project,
    string SurveyType,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null
);
