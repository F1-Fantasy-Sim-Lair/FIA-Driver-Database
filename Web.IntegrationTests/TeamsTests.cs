using FluentAssertions;
using FluentAssertions.Json;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Web.Authorization;
using Web.IntegrationTests.Common;

namespace Web.IntegrationTests;
public class TeamsTests
{
    [Test]
    public async Task CreateTeam_WithoutAuthorization_Returns401Unauthorized()
    {
        // Arrange
        using var client = new WebApplicationFactory().CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "teams")
        {
            Content = JsonContent.Create(new { name = "Team 1" })
        };

        // Act
        using var response = await client.SendAsync(request);

        // Assert
        response.Should().Be401Unauthorized();
    }

    [Test]
    public async Task TeamsListSerializesCorrectly()
    {
        // Arrange
        using var factory = new WebApplicationFactory();
        await factory.TestUsers.CreateUser("director1", Roles.DirectorRole);
        using var client = factory.CreateAuthenticatedClient(await factory.TestUsers.GetPrincipal("director1"));
        using var request = new HttpRequestMessage(HttpMethod.Post, "teams")
        {
            Content = JsonContent.Create(new { name = "Team 1" })
        };

        // Act
        using var response = await client.SendAsync(request);

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadFromJsonAsync<JsonObject>();
        content.Should().NotBeNull().And.Contain(kvp => kvp.Key == "name" && JsonNode.DeepEquals(kvp.Value, "Team 1"));

        using var teamsRequest = new HttpRequestMessage(HttpMethod.Get, "teams");
        using var teamsResponse = await client.SendAsync(teamsRequest);
        teamsResponse.Should().BeSuccessful();
        var teamsContent = await teamsResponse.Content.ReadFromJsonAsync<JsonArray>();
        teamsContent.Should().NotBeNull().And.HaveCount(1);
        teamsContent![0].Should().NotBeNull().And.BeOfType<JsonObject>().Which.Should().Contain(kvp => kvp.Key == "name" && JsonNode.DeepEquals(kvp.Value, "Team 1"));
    }
}
