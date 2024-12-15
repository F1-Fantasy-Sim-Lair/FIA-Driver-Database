using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Web.Model.EF;

namespace Web.IntegrationTests.Common;
internal class WebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var configDict = new Dictionary<string, string?>
        {
            { "ConnectionStrings:DriverDatabaseContext", "Data Source=fiadriverdatabase.db" },
            { "Database:MigrateOnStartup", "false" }
        };
        builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(configDict));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<DriverDatabaseContext>().Database.EnsureDeleted();
        scope.ServiceProvider.GetRequiredService<DriverDatabaseContext>().Database.EnsureCreated();
        return host;
    }
}
