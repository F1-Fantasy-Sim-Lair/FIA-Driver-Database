using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers;

public class AuthorizeController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IUserStore<IdentityUser> userStore) : ControllerBase
{
    readonly SignInManager<IdentityUser> signInManager = signInManager;
    readonly UserManager<IdentityUser> userManager = userManager;
    readonly IUserStore<IdentityUser> userStore = userStore;

    [HttpGet("/login"), AllowAnonymous]
    public IActionResult Login([FromQuery] string returnUrl = "/")
    {
        var redirectUri = Url.Action(nameof(LoginCompleted), new { returnUrl });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(DiscordAuthenticationDefaults.AuthenticationScheme, redirectUri);
        return Challenge(properties, DiscordAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet("/login-callback"), ApiExplorerSettings(IgnoreApi = true), AllowAnonymous]
    public async Task<IActionResult> LoginCompleted([FromQuery] string returnUrl = "/")
    {
        if (await signInManager.GetExternalLoginInfoAsync() is not { AuthenticationProperties: not null } info)
            return BadRequest();

        var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
        if (signInResult.Succeeded)
        {
            return Redirect(returnUrl);
        }
        else if (signInResult.IsLockedOut || signInResult.IsNotAllowed)
        {
            return StatusCode(403);
        }
        else if (signInResult.RequiresTwoFactor)
        {
            // There's no two-factor mechanism enabled in the application, so always return an error
            return BadRequest();
        }
        else // The user has no account yet
        {
            var user = new IdentityUser();
            await userStore.SetUserNameAsync(user, info.Principal.FindFirstValue(ClaimTypes.Name), CancellationToken.None);
            // The error cases below should ideally be handled gracefully
            // But for now there's no reason why they should occur other than application errors
            if (await userManager.CreateAsync(user) is { Succeeded: false })
                throw new Exception();

            if (await userManager.AddLoginAsync(user, info) is { Succeeded: false })
                throw new Exception();

            if (await userManager.AddClaimsAsync(user, info.Principal.Claims) is { Succeeded: false })
                throw new Exception();

            await signInManager.SignInAsync(user, info.AuthenticationProperties, DiscordAuthenticationDefaults.AuthenticationScheme);
            return Redirect(returnUrl);
        }
    }

    [HttpGet("/logout"), Authorize]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Redirect("/");
    }
}
