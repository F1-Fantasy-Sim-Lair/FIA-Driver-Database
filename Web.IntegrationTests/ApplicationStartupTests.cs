using FluentAssertions;
using Web.IntegrationTests.Common;

namespace Web.IntegrationTests;

public class ApplicationStartupTests
{
    [Test]
    public async Task ApplicationRespondsToRequests()
    {
        using var client = new WebApplicationFactory().CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Get, "swagger/index.html");
        var resp = await client.SendAsync(req);
        resp.Should().BeSuccessful();
    }
}