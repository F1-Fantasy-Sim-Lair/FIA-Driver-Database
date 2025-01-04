using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using Web.Model.EF;

namespace Web.Authorization;

static class DriverDatabaseIdentityExtensions
{
    public static IServiceCollection AddDriverDatabaseIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<DriverDatabaseContext>();

        services.ConfigureApplicationCookie(opts =>
        {
            opts.LoginPath = "/login";
            opts.Events.OnValidatePrincipal = ProxyEvent(ValidateRemotePrincipal, opts.Events.OnValidatePrincipal);
            opts.Events.OnRedirectToLogin = context =>
            {
                context.Response.Headers.WWWAuthenticate = "Discord";
                context.Response.Headers.Location = context.RedirectUri;
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            };
        });

        services.ConfigureExternalCookie(opts =>
        {
            // It's safe to keep the expiry cookie for a long time, because the refresh token is validated each day
            opts.ExpireTimeSpan = TimeSpan.FromDays(180);
            opts.SlidingExpiration = true;
        });

        services.AddAuthentication()
            .AddDiscord(options =>
            {
                var discordAuthSection = configuration.GetSection("Authentication:Discord");
                options.ClientId = discordAuthSection["ClientId"] ?? string.Empty;
                options.ClientSecret = discordAuthSection["ClientSecret"] ?? string.Empty;
                options.SaveTokens = true; // Refresh tokens need to be remembered
                options.Prompt = "none"; // Does not require explicit action from the user when the user has authenticated the application in the past
            });

        return services;
    }

    /// <summary>
    /// Validates the User Principal against Discord
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    static async Task ValidateRemotePrincipal(CookieValidatePrincipalContext context)
    {
        // If the token is at least a day old, refresh access token
        // This ensures that users can't use a retracted token for too long
        // A day is honestly still quite long, so this should probably be less
        if (context.Properties.IssuedUtc < TimeProvider.System.GetUtcNow().AddDays(-1)
            && context.Properties.GetTokenValue("refresh_token") is string refreshToken
            && !string.IsNullOrEmpty(refreshToken))
        {
            var discordOptions = context.HttpContext.RequestServices.GetRequiredService<IOptionsSnapshot<DiscordAuthenticationOptions>>().Get(DiscordAuthenticationDefaults.AuthenticationScheme);

            var body = new Dictionary<string, string> {
                { "client_id", discordOptions.ClientId },
                { "client_secret", discordOptions.ClientSecret },
                { "refresh_token", refreshToken },
                { "grant_type", "refresh_token" },
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, discordOptions.TokenEndpoint) { Content = new FormUrlEncodedContent(body) };
            using var response = await discordOptions.Backchannel.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                // On a bad request status the refresh code was likely invalid
                // At this point it should be assumed the user has retracted permission, and should not be authorized in case of account abuse
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    context.RejectPrincipal();

                context.Properties.UpdateTokenValue("refresh_token", string.Empty);
                return;
            }

            var tokenResponse = JsonSerializer.Deserialize<DiscordTokenResponse>(await response.Content.ReadAsStreamAsync())!;
            context.Properties.UpdateTokenValue("access_token", tokenResponse.AccessToken);
            if (tokenResponse.RefreshToken is not null)
                context.Properties.UpdateTokenValue("refresh_token", tokenResponse.RefreshToken);
            context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.Add(tokenResponse.ExpiresIn);

            context.ShouldRenew = true;
        }
    }

    static Func<T, Task> ProxyEvent<T>(Func<T, Task> newHandler, Func<T, Task> originalHandler)
    {
        return async (context) =>
        {
            if (newHandler != null)
                await newHandler(context);
            if (originalHandler != null)
                await originalHandler(context);
        };
    }

    record class DiscordTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("expires_in")] int ExpiresInSeconds)
    {
        public TimeSpan ExpiresIn => TimeSpan.FromSeconds(ExpiresInSeconds);
    }
}