using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using Web.IntegrationTests.Common;

namespace Web.IntegrationTests;
internal class DriversTests
{
    [Test]
    public async Task DriversAreStoredBetweenRequests()
    {
        using var client = new WebApplicationFactory().CreateClient();
        using var postReq = new HttpRequestMessage(HttpMethod.Post, "drivers")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { name = "Driver 1" }), new MediaTypeHeaderValue("application/json"))
        };

        using var postResp = await client.SendAsync(postReq);
        postResp.Should().BeSuccessful();

        using var listReq = new HttpRequestMessage(HttpMethod.Get, "drivers");
        using var listResp = await client.SendAsync(listReq);
        listResp.Should().BeSuccessful()
            .And.Subject.Content.Should().NotBeNull();

        var responseContent = JToken.Parse(await listResp.Content.ReadAsStringAsync());
        responseContent.Should().BeOfType<JArray>()
            .Which.Should().HaveCount(1);
    }
}
