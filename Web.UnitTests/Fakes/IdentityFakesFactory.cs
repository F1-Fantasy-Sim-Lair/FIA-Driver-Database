using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Security.Claims;

namespace Web.UnitTests.Fakes;
internal class IdentityFakesFactory
{
    public static SignInManager<IdentityUser> CreateSignInManager()
    {
        return CreateSignInManager(CreateUserManager());
    }

    public static SignInManager<IdentityUser> CreateSignInManager(UserManager<IdentityUser> userManager)
    {
        return Substitute.For<SignInManager<IdentityUser>>(
            userManager,
            new FakeHttpContextAccessor(),
            new UserClaimsPrincipalFactory<IdentityUser>(userManager, new FakeOptionsAccessor<IdentityOptions>()),
            new FakeOptionsAccessor<IdentityOptions>(),
            new NullLogger<SignInManager<IdentityUser>>(),
            new AuthenticationSchemeProvider(new FakeOptionsAccessor<AuthenticationOptions>()),
            new DefaultUserConfirmation<IdentityUser>());
    }

    public static UserManager<IdentityUser> CreateUserManager()
    {
        return CreateUserManager(CreateUserStore());
    }

    public static UserManager<IdentityUser> CreateUserManager(IUserStore<IdentityUser> userStore)
    {
        return Substitute.For<FakeUserManager<IdentityUser>>(
            userStore,
            new FakeOptionsAccessor<IdentityOptions>(),
            new PasswordHasher<IdentityUser>(),
            Array.Empty<IUserValidator<IdentityUser>>(),
            Array.Empty<IPasswordValidator<IdentityUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new ServiceCollection().BuildServiceProvider(),
            new NullLogger<UserManager<IdentityUser>>());
    }

    public static IUserStore<IdentityUser> CreateUserStore()
    {
        return Substitute.For<FakeUserStore>();
    }

    public static (SignInManager<IdentityUser>, UserManager<IdentityUser>, IUserStore<IdentityUser>) CreateIdentityServices()
    {
        var userStore = CreateUserStore();
        var userManager = CreateUserManager(userStore);
        var signInManager = CreateSignInManager(userManager);
        return (signInManager, userManager, userStore);
    }
}

public class FakeSignInManager<TUser> : SignInManager<TUser> where TUser : class
{
    public FakeSignInManager(
        UserManager<TUser> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<TUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<TUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<TUser> confirmation)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
    }
}

public class FakeUserManager<TUser> : UserManager<TUser> where TUser : class
{
    public FakeUserManager(
        IUserStore<TUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<TUser> passwordHasher,
        IEnumerable<IUserValidator<TUser>> userValidators,
        IEnumerable<IPasswordValidator<TUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<TUser>> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
    }
}

class FakeHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; }
}

class FakeHttpContext : HttpContext
{
    public override IServiceProvider RequestServices { get; set; }

    public override IFeatureCollection Features => throw new NotImplementedException();

    public override HttpRequest Request => throw new NotImplementedException();

    public override HttpResponse Response => throw new NotImplementedException();

    public override ConnectionInfo Connection => throw new NotImplementedException();

    public override WebSocketManager WebSockets => throw new NotImplementedException();

    public override ClaimsPrincipal User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override IDictionary<object, object?> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override CancellationToken RequestAborted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override void Abort() => throw new NotImplementedException();
}

class FakeOptionsAccessor<TOptions> : IOptions<TOptions> where TOptions : class, new()
{
    readonly TOptions _value;
    public FakeOptionsAccessor(TOptions? value = null)
    {
        _value = value ?? new TOptions();
    }
    public TOptions Value => _value;
}

public class FakeUserStore : IUserStore<IdentityUser>
{
    public Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }
    public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }
    public void Dispose()
    {
        return;
    }
    public Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    public Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    public Task<string> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    public Task<string> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    public Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    public Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    public Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }
}
