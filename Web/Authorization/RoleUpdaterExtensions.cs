using System.Threading.Channels;
using Web.Authorization.RoleAutoUpdates;

namespace Web.Authorization;

public static class RoleUpdaterExtensions
{
    /// <summary>
    /// Adds automatic role updates.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration">A <see cref="IConfigurationSection"/> that can bind to a <see cref="RolesConfiguration"/> instance.</param>
    /// <returns></returns>
    public static IServiceCollection AddAutomaticRoleUpdates(this IServiceCollection services, IConfigurationSection configuration)
    {
        services.AddOptions<RolesConfiguration>()
            .Bind(configuration)
            .ValidateDataAnnotations();

        services.AddSingleton(Channel.CreateUnbounded<RolesConfiguration>());
        services.AddSingleton(sp => sp.GetRequiredService<Channel<RolesConfiguration>>().Reader);
        services.AddSingleton(sp => sp.GetRequiredService<Channel<RolesConfiguration>>().Writer);
        services.AddHostedService<PropagateRolesConfigurationChangesHostedService>();
        services.AddHostedService<GenerateRolesHostedService>();
        return services;
    }
}
