using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EnergyBoatApp.Tests.ContractTests;

/// <summary>
/// Contract tests for GET /api/boats endpoint.
/// Verifies response schema matches contracts/get-boats.md specification.
/// MUST match exactly - any changes break frontend compatibility.
/// </summary>
public class GetBoatsContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GetBoatsContractTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetBoats_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/boats");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetBoats_ReturnsArrayOfBoatStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/boats");
        var boats = await response.Content.ReadFromJsonAsync<List<BoatStatus>>();

        // Assert
        Assert.NotNull(boats);
        Assert.IsType<List<BoatStatus>>(boats);
    }

    [Fact]
    public async Task GetBoats_ReturnsFourBoats()
    {
        // Act
        var response = await _client.GetAsync("/api/boats");
        var boats = await response.Content.ReadFromJsonAsync<List<BoatStatus>>();

        // Assert
        Assert.NotNull(boats);
        Assert.Equal(4, boats.Count);
    }

    [Fact]
    public async Task GetBoats_EachBoatHasAllRequiredFields()
    {
        // Act
        var response = await _client.GetAsync("/api/boats");
        var boats = await response.Content.ReadFromJsonAsync<List<BoatStatus>>();

        // Assert
        Assert.NotNull(boats);
        foreach (var boat in boats)
        {
            // Required field: Id
            Assert.NotNull(boat.Id);
            Assert.NotEmpty(boat.Id);
            Assert.Matches(@"^BOAT-\d{3}$", boat.Id);

            // Required field: Latitude (-90 to 90)
            Assert.InRange(boat.Latitude, -90, 90);

            // Required field: Longitude (-180 to 180)
            Assert.InRange(boat.Longitude, -180, 180);

            // Required field: Status (enum)
            Assert.Contains(boat.Status, new[] { "Active", "Charging", "Maintenance" });

            // Required field: EnergyLevel (0-100)
            Assert.InRange(boat.EnergyLevel, 0, 100);

            // Required field: VesselName
            Assert.NotNull(boat.VesselName);
            Assert.NotEmpty(boat.VesselName);

            // Required field: SurveyType
            Assert.NotNull(boat.SurveyType);
            Assert.NotEmpty(boat.SurveyType);

            // Required field: Project
            Assert.NotNull(boat.Project);
            Assert.NotEmpty(boat.Project);

            // Required field: Equipment
            Assert.NotNull(boat.Equipment);
            Assert.NotEmpty(boat.Equipment);

            // Required field: AreaCovered (>= 0)
            Assert.True(boat.AreaCovered >= 0);

            // Required field: Speed
            Assert.NotNull(boat.Speed);
            Assert.NotEmpty(boat.Speed);

            // Required field: CrewCount (> 0)
            Assert.True(boat.CrewCount > 0);

            // Required field: Conditions
            Assert.NotNull(boat.Conditions);
            Assert.NotEmpty(boat.Conditions);

            // Required field: Heading (0-360)
            Assert.InRange(boat.Heading, 0, 360);
        }
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(0.5)]
    [InlineData(10.0)]
    public async Task GetBoats_WithSpeedParameter_ReturnsSuccess(double speed)
    {
        // Act
        var response = await _client.GetAsync($"/api/boats?speed={speed}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var boats = await response.Content.ReadFromJsonAsync<List<BoatStatus>>();
        Assert.NotNull(boats);
        Assert.Equal(4, boats.Count);
    }

    [Theory]
    [InlineData(0.05)]  // Below minimum
    [InlineData(11.0)]  // Above maximum
    public async Task GetBoats_WithInvalidSpeed_ReturnsBadRequest(double speed)
    {
        // Act
        var response = await _client.GetAsync($"/api/boats?speed={speed}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBoats_VerifyBoat001Exists()
    {
        // Act
        var response = await _client.GetAsync("/api/boats");
        var boats = await response.Content.ReadFromJsonAsync<List<BoatStatus>>();

        // Assert
        Assert.NotNull(boats);
        var boat001 = boats.FirstOrDefault(b => b.Id == "BOAT-001");
        Assert.NotNull(boat001);
        Assert.Equal("Contoso Sea Voyager", boat001.VesselName);
        Assert.Equal("Geophysical Survey", boat001.SurveyType);
    }

    [Fact]
    public async Task GetBoats_AllBoatsHaveUniqueIds()
    {
        // Act
        var response = await _client.GetAsync("/api/boats");
        var boats = await response.Content.ReadFromJsonAsync<List<BoatStatus>>();

        // Assert
        Assert.NotNull(boats);
        var ids = boats.Select(b => b.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }
}

/// <summary>
/// BoatStatus record matching contracts/get-boats.md schema exactly.
/// Any changes to this record indicate a breaking API change.
/// </summary>
public record BoatStatus(
    string Id,
    double Latitude,
    double Longitude,
    string Status,
    double EnergyLevel,
    string VesselName,
    string SurveyType,
    string Project,
    string Equipment,
    double AreaCovered,
    string Speed,
    int CrewCount,
    string Conditions,
    double Heading
);
