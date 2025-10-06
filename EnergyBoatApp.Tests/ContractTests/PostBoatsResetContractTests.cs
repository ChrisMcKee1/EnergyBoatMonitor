using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EnergyBoatApp.Tests.ContractTests;

/// <summary>
/// Contract tests for POST /api/boats/reset endpoint.
/// Verifies response schema matches contracts/post-boats-reset.md specification.
/// MUST match exactly - any changes break API contract.
/// </summary>
public class PostBoatsResetContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PostBoatsResetContractTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ResetBoats_ReturnsSuccess()
    {
        // Act
        var response = await _client.PostAsync("/api/boats/reset", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ResetBoats_ReturnsResetResponse()
    {
        // Act
        var response = await _client.PostAsync("/api/boats/reset", null);
        var resetResponse = await response.Content.ReadFromJsonAsync<ResetResponse>();

        // Assert
        Assert.NotNull(resetResponse);
        Assert.IsType<ResetResponse>(resetResponse);
    }

    [Fact]
    public async Task ResetBoats_ReturnsSuccessTrue()
    {
        // Act
        var response = await _client.PostAsync("/api/boats/reset", null);
        var resetResponse = await response.Content.ReadFromJsonAsync<ResetResponse>();

        // Assert
        Assert.NotNull(resetResponse);
        Assert.True(resetResponse.Success);
    }

    [Fact]
    public async Task ResetBoats_ReturnsCorrectMessage()
    {
        // Act
        var response = await _client.PostAsync("/api/boats/reset", null);
        var resetResponse = await response.Content.ReadFromJsonAsync<ResetResponse>();

        // Assert
        Assert.NotNull(resetResponse);
        Assert.NotNull(resetResponse.Message);
        Assert.NotEmpty(resetResponse.Message);
        Assert.Contains("reset", resetResponse.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ResetBoats_ResetsAllFourBoats()
    {
        // Act
        var response = await _client.PostAsync("/api/boats/reset", null);
        var resetResponse = await response.Content.ReadFromJsonAsync<ResetResponse>();

        // Assert
        Assert.NotNull(resetResponse);
        Assert.Equal(4, resetResponse.BoatsReset);
    }

    [Fact]
    public async Task ResetBoats_AllFieldsPopulated()
    {
        // Act
        var response = await _client.PostAsync("/api/boats/reset", null);
        var resetResponse = await response.Content.ReadFromJsonAsync<ResetResponse>();

        // Assert - verify all 3 required fields are present
        Assert.NotNull(resetResponse);
        Assert.True(resetResponse.Success);
        Assert.NotNull(resetResponse.Message);
        Assert.True(resetResponse.BoatsReset > 0);
    }

    [Fact]
    public async Task ResetBoats_ThenGetBoats_ShowsInitialState()
    {
        // Arrange - reset boats
        await _client.PostAsync("/api/boats/reset", null);

        // Act - get boat states
        var getResponse = await _client.GetAsync("/api/boats");
        var boats = await getResponse.Content.ReadFromJsonAsync<List<BoatStatus>>();

        // Assert - verify BOAT-001 is at initial position
        Assert.NotNull(boats);
        var boat001 = boats.FirstOrDefault(b => b.Id == "BOAT-001");
        Assert.NotNull(boat001);
        Assert.Equal(51.5074, boat001.Latitude, precision: 4);
        Assert.Equal(-0.1278, boat001.Longitude, precision: 4);
        Assert.Equal(45.0, boat001.Heading, precision: 1);
        Assert.Equal(85.5, boat001.EnergyLevel, precision: 1);
        Assert.Equal("Active", boat001.Status);
    }

    [Fact]
    public async Task ResetBoats_ThenGetBoats_AllBoatsAtInitialPositions()
    {
        // Arrange - reset boats
        await _client.PostAsync("/api/boats/reset", null);

        // Act - get boat states
        var getResponse = await _client.GetAsync("/api/boats");
        var boats = await getResponse.Content.ReadFromJsonAsync<List<BoatStatus>>();

        // Assert - verify all 4 boats have expected initial states
        Assert.NotNull(boats);
        Assert.Equal(4, boats.Count);

        var boat001 = boats.First(b => b.Id == "BOAT-001");
        Assert.Equal("Active", boat001.Status);
        Assert.True(boat001.EnergyLevel > 70); // Active boats have high energy

        var boat002 = boats.First(b => b.Id == "BOAT-002");
        Assert.Equal("Charging", boat002.Status);

        var boat003 = boats.First(b => b.Id == "BOAT-003");
        Assert.Equal("Active", boat003.Status);

        var boat004 = boats.First(b => b.Id == "BOAT-004");
        Assert.Equal("Maintenance", boat004.Status);
        Assert.True(boat004.EnergyLevel < 20); // Maintenance boats have low energy
    }

    [Fact]
    public async Task ResetBoats_MultipleCallsSucceed()
    {
        // Act - call reset twice
        var response1 = await _client.PostAsync("/api/boats/reset", null);
        var response2 = await _client.PostAsync("/api/boats/reset", null);

        // Assert - both succeed
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var resetResponse1 = await response1.Content.ReadFromJsonAsync<ResetResponse>();
        var resetResponse2 = await response2.Content.ReadFromJsonAsync<ResetResponse>();

        Assert.NotNull(resetResponse1);
        Assert.NotNull(resetResponse2);
        Assert.Equal(4, resetResponse1.BoatsReset);
        Assert.Equal(4, resetResponse2.BoatsReset);
    }
}

/// <summary>
/// ResetResponse record matching contracts/post-boats-reset.md schema exactly.
/// Any changes to this record indicate a breaking API change.
/// </summary>
public record ResetResponse(
    bool Success,
    string Message,
    int BoatsReset
);
