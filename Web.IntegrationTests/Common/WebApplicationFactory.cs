using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Security.Claims;
using Web.Model.EF;

namespace Web.IntegrationTests.Common;
internal class WebApplicationFactory : WebApplicationFactory<Program>
{
    const string TestConnectionString = "Data Source=fiadriverdatabase.db";

    public WebApplicationFactory() : base()
    {
        TestUsers = new TestUserManager(this, Services);
    }

    public TestUserManager TestUsers { get; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var configDict = new Dictionary<string, string?>
        {
            { "ConnectionStrings:DriverDatabaseContext", TestConnectionString },
            { "Database:MigrateOnStartup", "false" }
        };
        builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(configDict));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        using var dbContext = new DriverDatabaseContext(new DbContextOptionsBuilder<DriverDatabaseContext>()
            .UseSqlite(TestConnectionString)
            .Options);
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        return base.CreateHost(builder);
    }

    public HttpClient CreateAuthenticatedClient(ClaimsPrincipal principal)
        => CreateAuthenticatedClient(principal, new());

    public HttpClient CreateAuthenticatedClient(ClaimsPrincipal principal, WebApplicationFactoryClientOptions clientOptions)
    {
        using var serviceScope = Services.CreateScope();
        var authProperties = new AuthenticationProperties()
        {
            ExpiresUtc = DateTimeOffset.Now.AddDays(1),
            IssuedUtc = DateTimeOffset.Now,
            Items =
            {
                new KeyValuePair<string, string?>(".AuthScheme", "Discord"),
                new KeyValuePair<string, string?>("LoginProvider", "Discord"),
                new KeyValuePair<string, string?>(".Token.access_token", "abcdefghijklmnopqrstuvwxyz"),
                new KeyValuePair<string, string?>(".Token.refresh_token", "1234567890"),
                new KeyValuePair<string, string?>(".Token.token_type", "Bearer"),
                new KeyValuePair<string, string?>(".TokenNames", "access_token;refresh_token;token_type;expires_at"),
                new KeyValuePair<string, string?>(".Token.expires_at", DateTimeOffset.Now.AddDays(1).ToString("O"))
            }
        };

        var cookieValueBytes = serviceScope.ServiceProvider.GetRequiredService<IDataProtectionProvider>()
            .CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", IdentityConstants.ApplicationScheme, "v2")
            .Protect(TicketSerializer.Default.Serialize(new AuthenticationTicket(principal, authProperties, IdentityConstants.ApplicationScheme)));

        var cookieName = CookieAuthenticationDefaults.CookiePrefix + Uri.EscapeDataString(IdentityConstants.ApplicationScheme);
        var cookieValue = Base64UrlTextEncoder.Encode(cookieValueBytes);

        var cookieContainer = new CookieContainer();

        var setCookieHeader = new SetCookieHeaderValue(cookieName, cookieValue)
        {
            Path = "/",
            HttpOnly = true,
            Secure = false,
            Expires = DateTimeOffset.Now.AddDays(1),
            Domain = clientOptions.BaseAddress.Host,
            SameSite = SameSiteMode.None
        };

        cookieContainer.SetCookies(clientOptions.BaseAddress, setCookieHeader.ToString());

        return CreateDefaultClient(clientOptions.BaseAddress, CreateHandlersCore().ToArray());

        IEnumerable<DelegatingHandler> CreateHandlersCore()
        {
            if (clientOptions.AllowAutoRedirect)
            {
                yield return new RedirectHandler(clientOptions.MaxAutomaticRedirections);
            }

            yield return new CookieContainerHandler(cookieContainer);
        }
    }
}
