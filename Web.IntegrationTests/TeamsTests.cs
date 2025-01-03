using FluentAssertions;
using Web.IntegrationTests.Common;

namespace Web.IntegrationTests;
public class TeamsTests
{
    [Test]
    public async Task GetAllTeams_WithoutAuthorization_Returns401Unauthorized()
    {
        // Arrange
        using var client = new WebApplicationFactory().CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "teams");

        // Act
        using var response = await client.SendAsync(request);

        // Assert
        response.Should().Be401Unauthorized();
    }

    [Test]
    public async Task GetAllTeams_Returns200OK()
    {
        // Arrange
        using var factory = new WebApplicationFactory();
        using var client = factory.CreateAuthenticatedClient(await factory.TestUsers.GetPrincipal("testuser"));
        using var request = new HttpRequestMessage(HttpMethod.Get, "teams");

        // Act
        using var response = await client.SendAsync(request);

        // Assert
        response.Should().BeSuccessful();
    }
}
