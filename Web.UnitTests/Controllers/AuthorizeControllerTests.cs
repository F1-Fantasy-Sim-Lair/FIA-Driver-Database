using AspNet.Security.OAuth.Discord;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.Security.Claims;
using Web.Controllers;
using Web.UnitTests.Fakes;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Web.UnitTests.Controllers;
internal class AuthorizeControllerTests
{
    class FakeUriHelper : IUrlHelper
    {
        public ActionContext ActionContext => throw new NotImplementedException();
        public IServiceProvider Services => throw new NotImplementedException();
        public string Action(UrlActionContext actionContext)
        {
            return "/login-callback";
        }
        public string Content(string contentPath)
        {
            throw new NotImplementedException();
        }
        public bool IsLocalUrl(string url)
        {
            throw new NotImplementedException();
        }
        public string Link(string routeName, object values)
        {
            throw new NotImplementedException();
        }
        public string RouteUrl(UrlRouteContext routeContext)
        {
            throw new NotImplementedException();
        }
    }

    [Test]
    public async Task Login_RedirectsToDiscord()
    {
        var (signInManager, userManager, userStore) = IdentityFakesFactory.CreateIdentityServices();
        var sut = new AuthorizeController(signInManager, userManager, userStore)
        {
            Url = new FakeUriHelper()
        };
        var result = (sut.Login()).Should().BeOfType<ChallengeResult>().Subject;
        result.AuthenticationSchemes.Should().Contain(DiscordAuthenticationDefaults.AuthenticationScheme);
    }

    [Test]
    public async Task LoginCompleted_MissingExternalLoginInfo_ReturnsBadRequest()
    {
        var (signInManager, userManager, userStore) = IdentityFakesFactory.CreateIdentityServices();
        signInManager.GetExternalLoginInfoAsync().ReturnsNull();
        var sut = new AuthorizeController(signInManager, userManager, userStore)
        {
            Url = new FakeUriHelper()
        };
        (await sut.LoginCompleted()).Should().BeOfType<BadRequestResult>();
    }

    [Test]
    public async Task LoginCompleted_ExistingUser_RedirectsToReturnUrl()
    {
        var (signInManager, userManager, userStore) = IdentityFakesFactory.CreateIdentityServices();
        signInManager.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), DiscordAuthenticationDefaults.AuthenticationScheme, "1", "Discord") { AuthenticationProperties = new() });
        signInManager.ExternalLoginSignInAsync("Discord", "1", true, true).Returns(SignInResult.Success);
        var sut = new AuthorizeController(signInManager, userManager, userStore)
        {
            Url = new FakeUriHelper()
        };
        var result = (await sut.LoginCompleted()).Should().BeOfType<RedirectResult>().Subject;
        result.Url.Should().Be("/");
    }

    [Test, TestCaseSource(typeof(AuthorizeControllerTests), nameof(LoginCompleted_LockedOutOrNotAllowedUser_ReturnsForbidden_Cases))]
    public async Task LoginCompleted_LockedOutOrNotAllowedUser_ReturnsForbidden(SignInResult signInResult)
    {
        var (signInManager, userManager, userStore) = IdentityFakesFactory.CreateIdentityServices();
        signInManager.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), DiscordAuthenticationDefaults.AuthenticationScheme, "1", "Discord") { AuthenticationProperties = new() });
        signInManager.ExternalLoginSignInAsync("Discord", "1", true, true).Returns(signInResult);
        var sut = new AuthorizeController(signInManager, userManager, userStore)
        {
            Url = new FakeUriHelper()
        };
        var result = (await sut.LoginCompleted()).Should().BeOfType<StatusCodeResult>().Subject;
        result.StatusCode.Should().Be(403);
    }

    [Test]
    public async Task LoginCompleted_TwoFactorEnabledUser_ReturnsBadRequest()
    {
        var (signInManager, userManager, userStore) = IdentityFakesFactory.CreateIdentityServices();
        signInManager.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Test") })), DiscordAuthenticationDefaults.AuthenticationScheme, "1", "Discord") { AuthenticationProperties = new() });
        signInManager.ExternalLoginSignInAsync("Discord", "1", true, true).Returns(SignInResult.TwoFactorRequired);
        var sut = new AuthorizeController(signInManager, userManager, userStore)
        {
            Url = new FakeUriHelper()
        };
        (await sut.LoginCompleted()).Should().BeOfType<BadRequestResult>();
    }

    [Test]
    public async Task LoginCompleted_NewUser_CreatesUserAndRedirectsToReturnUrl()
    {
        var (signInManager, userManager, userStore) = IdentityFakesFactory.CreateIdentityServices();
        signInManager.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "Test")])), DiscordAuthenticationDefaults.AuthenticationScheme, "1", "Discord") { AuthenticationProperties = new() });
        signInManager.ExternalLoginSignInAsync("Discord", "1", true, true).Returns(SignInResult.Failed); // 'failed' really means 'user does not exist'
        signInManager.SignInAsync(Arg.Any<IdentityUser>(), Arg.Any<AuthenticationProperties>(), DiscordAuthenticationDefaults.AuthenticationScheme).Returns(Task.CompletedTask);
        userManager.CreateAsync(Arg.Any<IdentityUser>()).Returns(IdentityResult.Success);
        userManager.AddLoginAsync(Arg.Any<IdentityUser>(), Arg.Any<ExternalLoginInfo>()).Returns(IdentityResult.Success);
        userManager.AddClaimsAsync(Arg.Any<IdentityUser>(), Arg.Any<IEnumerable<Claim>>()).Returns(IdentityResult.Success);
        var sut = new AuthorizeController(signInManager, userManager, userStore)
        {
            Url = new FakeUriHelper()
        };
        var result = (await sut.LoginCompleted()).Should().BeOfType<RedirectResult>().Subject;
        result.Url.Should().Be("/");
    }

    static IEnumerable<TestCaseData> LoginCompleted_LockedOutOrNotAllowedUser_ReturnsForbidden_Cases()
    {
        yield return new(SignInResult.LockedOut);
        yield return new(SignInResult.NotAllowed);
    }
}
