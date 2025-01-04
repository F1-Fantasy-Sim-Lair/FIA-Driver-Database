using Microsoft.AspNetCore.Identity;
using System.Threading.Channels;

namespace Web.Authorization.RoleAutoUpdates;

class GenerateRolesHostedService(
    IServiceProvider serviceProvider,
    ChannelReader<RolesConfiguration> reader) : BackgroundService
{
    readonly IServiceProvider serviceProvider = serviceProvider;
    readonly ChannelReader<RolesConfiguration> reader = reader;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await reader.ReadAsync(stoppingToken) is RolesConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var existingRoles = roleManager.Roles
                .Select(r => r.Name)
                .Where(r => r != null)
                .OfType<string>()
                .ToArray();

            var newRoles = configuration.ApplicationRoles.Except(existingRoles);
            foreach (var role in newRoles)
                await roleManager.CreateAsync(new IdentityRole(role));

            var roles = roleManager.Roles.ToList();
        }
    }
}
