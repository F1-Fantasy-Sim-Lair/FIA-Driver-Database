using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Web.IntegrationTests.Common;

internal class TestUserManager(WebApplicationFactory webApplicationFactory, IServiceProvider serviceProvider)
{
    readonly WebApplicationFactory webApplicationFactory = webApplicationFactory;
    readonly IServiceProvider serviceProvider = serviceProvider;

    public async Task<IdentityUser> CreateUser(string username)
    {
        using var serviceScope = serviceProvider.CreateScope();
        var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var currentUser = new IdentityUser();
        await userManager.SetUserNameAsync(currentUser, username);
        await userManager.CreateAsync(currentUser);
        return currentUser;
    }

    public async Task<ClaimsPrincipal> GetPrincipal(IdentityUser user)
    {
        using var serviceScope = serviceProvider.CreateScope();
        return await GetPrincipal(user, serviceScope.ServiceProvider);
    }

    public async Task<ClaimsPrincipal> GetPrincipal(string username)
    {
        using var serviceScope = serviceProvider.CreateScope();
        var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        return await GetPrincipal(await userManager.FindByNameAsync(username) ?? new IdentityUser("testuser"), serviceScope.ServiceProvider);
    }

    static async Task<ClaimsPrincipal> GetPrincipal(IdentityUser user, IServiceProvider serviceProvider)
    {
        var claimsPrincipalFactory = serviceProvider.GetRequiredService<IUserClaimsPrincipalFactory<IdentityUser>>();
        return await claimsPrincipalFactory.CreateAsync(user);
    }
}
