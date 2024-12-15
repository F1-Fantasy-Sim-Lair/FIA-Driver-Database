using AspNet.Security.OAuth.Discord;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Web.IntegrationTests.Common;

namespace Web.IntegrationTests;
internal class AuthorizeTests
{
    static readonly WebApplicationFactoryClientOptions clientOptions = new()
    {
        AllowAutoRedirect = false, // Don't follow redirects so they can be asserted
    };

    [Test]
    public async Task CanAuthorizeWithANewAccount()
    {
        using var client = new WebApplicationFactory()
            .WithWebHostBuilder(ConfigureDiscordAuthForTesting)
            .CreateClient(clientOptions);

        // First, initiate the Discord authentication flow
        // This will redirect to Discord's OAuth2 authorization endpoint
        // A correlation cookie is set, so this step is necessary even during testing
        using var initiateDiscordAuthReq = new HttpRequestMessage(HttpMethod.Get, "login");
        using var initiateDiscordAuthResp = await client.SendAsync(initiateDiscordAuthReq);
        initiateDiscordAuthResp.Should().BeRedirection();
        initiateDiscordAuthResp.Should().HaveHeader("Location");
        initiateDiscordAuthResp.Headers.Location.Should().NotBeNull()
            .And.StartWith("https://discord.com/api/oauth2/authorize");
        var discordToAuthHandlerRedirectUri = initiateDiscordAuthResp.Headers.Location.Should().HaveQueryParameter("redirect_uri").Subject;
        var state = initiateDiscordAuthResp.Headers.Location.Should().HaveQueryParameter("state").Subject;

        // Then, simulate the redirect from Discord to the authentication handler
        var authHandlerUri = new UriBuilder(Uri.UnescapeDataString(discordToAuthHandlerRedirectUri));
        authHandlerUri.Query += $"state={state}&code=1234";
        using var discordCallbackReq = new HttpRequestMessage(HttpMethod.Post, authHandlerUri.Uri);
        using var discordCallbackResp = await client.SendAsync(discordCallbackReq);
        discordCallbackResp.Should().BeRedirection();
        discordCallbackResp.Headers.Location.Should().StartWith("/login-callback")
            .And.HaveQueryParameter("returnUrl");

        // Finally, simulate the redirect from the authentication handler to the controller
        using var controllerCallbackReq = new HttpRequestMessage(HttpMethod.Get, discordCallbackResp.Headers.Location!);
        using var controllerCallbackResp = await client.SendAsync(controllerCallbackReq);
        controllerCallbackResp.Should().BeRedirection();
        controllerCallbackResp.Headers.Location.Should().NotBeNull();
    }

    [Test]
    public async Task CanLogoutWhenAuthorized()
    {
        using var client = new WebApplicationFactory()
            .WithWebHostBuilder(ConfigureDiscordAuthForTesting)
            .CreateClient(clientOptions);

        // Use a smaller version of the CanAuthorizeWithANewAccount test to authorize the user
        // This is necessary because the user must be authorized to log out
        // That behavior means this test doubles as an integration test for authorization checking
        using var initiateDiscordAuthReq = new HttpRequestMessage(HttpMethod.Get, "login");
        using var initiateDiscordAuthResp = await client.SendAsync(initiateDiscordAuthReq);
        var queryParams = UriHelpers.GetQueryParams(initiateDiscordAuthResp.Headers.Location!);

        var authHandlerUri = new UriBuilder(Uri.UnescapeDataString(queryParams["redirect_uri"]));
        authHandlerUri.Query += $"state={queryParams["state"]}&code=1234";
        using var discordCallbackReq = new HttpRequestMessage(HttpMethod.Post, authHandlerUri.Uri);
        using var discordCallbackResp = await client.SendAsync(discordCallbackReq);

        using var controllerCallbackReq = new HttpRequestMessage(HttpMethod.Get, discordCallbackResp.Headers.Location!);
        using var controllerCallbackResp = await client.SendAsync(controllerCallbackReq);

        // Now that the user is authorized, log out
        using var logoutReq = new HttpRequestMessage(HttpMethod.Get, "logout");
        using var logoutResp = await client.SendAsync(logoutReq);
        logoutResp.Should().BeRedirection();
    }

    [Test]
    public async Task CannotLogoutWhenNotAuthorized()
    {
        using var client = new WebApplicationFactory()
            .CreateClient(clientOptions);

        using var loginReq = new HttpRequestMessage(HttpMethod.Get, "logout");
        using var loginResp = await client.SendAsync(loginReq);
        loginResp.Should().Be401Unauthorized();
    }
    static void ConfigureDiscordAuthForTesting(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Configure<DiscordAuthenticationOptions>(DiscordAuthenticationDefaults.AuthenticationScheme, opts =>
            {
                opts.BackchannelHttpHandler = new DiscordAuthHttpMessageHandler();
                // The test server doesn't support HTTPS, so disable secure cookies
                opts.CorrelationCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None; 
                opts.CorrelationCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });
            services.ConfigureApplicationCookie(opts =>
            {
                // The test server doesn't support HTTPS, so disable secure cookies
                opts.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None;
                opts.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });
            services.ConfigureExternalCookie(opts =>
            {
                // The test server doesn't support HTTPS, so disable secure cookies
                opts.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None;
                opts.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });
        });
    }

    class DiscordAuthHttpMessageHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, object content) =>
                new(statusCode) { Content = JsonContent.Create(content) };

            if (request.RequestUri is not null && request.RequestUri.AbsoluteUri.StartsWith("https://discord.com/api/oauth2/token"))
            {
                var formData = await request.Content.ReadAsFormDataAsync(cancellationToken);
                if (formData["grant_type"] != "authorization_code")
                    return JsonResponse(HttpStatusCode.BadRequest, new { error = "invalid_request" });

                if (formData["code"] != "1234")
                    return JsonResponse(HttpStatusCode.BadRequest, new { error = "invalid_grant" });

                return JsonResponse(HttpStatusCode.OK, new
                {
                    access_token = "abcdefghijklmnopqrstuvwxyz",
                    refresh_token = "1234567890",
                    expires_in = 3600,
                    token_type = "Bearer",
                    scope = "identity"
                });
            }

            if (request.RequestUri is not null && request.RequestUri.AbsoluteUri.StartsWith("https://discord.com/api/users/@me"))
            {
                if (request.Headers.Authorization?.Scheme != "Bearer" || request.Headers.Authorization.Parameter != "abcdefghijklmnopqrstuvwxyz")
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);

                return JsonResponse(HttpStatusCode.OK, new
                {
                    id = "1",
                    username = "discord",
                    avatar = "abcd",
                    discriminator = "0",
                    global_name = "Discord",
                });
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}
