using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;

namespace MVCTemplate.Web.Test;

public static class MockHelper
{
    public static StringBuilder LogMessage = new StringBuilder();

    public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        mgr.Object.UserValidators.Add(new UserValidator<TUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());
        return mgr;
    }

    public static Mock<RoleManager<TRole>> MockRoleManager<TRole>(IRoleStore<TRole> store = null) where TRole : class
    {
        store = store ?? new Mock<IRoleStore<TRole>>().Object;
        var roles = new List<IRoleValidator<TRole>>();
        roles.Add(new RoleValidator<TRole>());
        return new Mock<RoleManager<TRole>>(store, roles, MockLookupNormalizer(),
            new IdentityErrorDescriber(), null);
    }

    public static UserManager<TUser> TestUserManager<TUser>(IUserStore<TUser> store = null) where TUser : class
    {
        store = store ?? new Mock<IUserStore<TUser>>().Object;
        var options = new Mock<IOptions<IdentityOptions>>();
        var idOptions = new IdentityOptions();
        idOptions.Lockout.AllowedForNewUsers = false;
        options.Setup(o => o.Value).Returns(idOptions);
        var userValidators = new List<IUserValidator<TUser>>();
        var validator = new Mock<IUserValidator<TUser>>();
        userValidators.Add(validator.Object);
        var pwdValidators = new List<PasswordValidator<TUser>>();
        pwdValidators.Add(new PasswordValidator<TUser>());
        var userManager = new UserManager<TUser>(store, options.Object, new PasswordHasher<TUser>(),
            userValidators, pwdValidators, MockLookupNormalizer(),
            new IdentityErrorDescriber(), null,
            new Mock<ILogger<UserManager<TUser>>>().Object);
        validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<TUser>()))
            .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();
        return userManager;
    }

    public static RoleManager<TRole> TestRoleManager<TRole>(IRoleStore<TRole> store = null) where TRole : class
    {
        store = store ?? new Mock<IRoleStore<TRole>>().Object;
        var roles = new List<IRoleValidator<TRole>>();
        roles.Add(new RoleValidator<TRole>());
        return new RoleManager<TRole>(store, roles,
            MockLookupNormalizer(),
            new IdentityErrorDescriber(),
            null);
    }

    public static ILookupNormalizer MockLookupNormalizer()
    {
        var normalizerFunc = new Func<string, string>(i =>
        {
            if (i == null)
            {
                return null;
            }
            else
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(i)).ToUpperInvariant();
            }
        });
        var lookupNormalizer = new Mock<ILookupNormalizer>();
        lookupNormalizer.Setup(i => i.NormalizeName(It.IsAny<string>())).Returns(normalizerFunc);
        lookupNormalizer.Setup(i => i.NormalizeEmail(It.IsAny<string>())).Returns(normalizerFunc);
        return lookupNormalizer.Object;
    }

    public static SignInManager<IdentityUser> SetupSignInManager(UserManager<IdentityUser> manager, HttpContext context, ILogger logger = null, IdentityOptions identityOptions = null, IAuthenticationSchemeProvider schemeProvider = null)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        contextAccessor.Setup(a => a.HttpContext).Returns(context);
        var roleManager = MockRoleManager<IdentityRole>();
        identityOptions = identityOptions ?? new IdentityOptions();
        var options = new Mock<IOptions<IdentityOptions>>();
        options.Setup(a => a.Value).Returns(identityOptions);
        var claimsFactory = new UserClaimsPrincipalFactory<IdentityUser, IdentityRole>(manager, roleManager.Object, options.Object);
        schemeProvider = schemeProvider ?? new Mock<IAuthenticationSchemeProvider>().Object;
        var sm = new SignInManager<IdentityUser>(manager, contextAccessor.Object, claimsFactory, options.Object, null, schemeProvider, new DefaultUserConfirmation<IdentityUser>());
        sm.Logger = logger ?? NullLogger<SignInManager<IdentityUser>>.Instance;
        return sm;
    }
    public static Mock<SignInManager<IdentityUser>> SetupSignInManagerMock(UserManager<IdentityUser> manager, HttpContext context, ILogger logger = null, IdentityOptions identityOptions = null, IAuthenticationSchemeProvider schemeProvider = null)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        contextAccessor.Setup(a => a.HttpContext).Returns(context);
        var roleManager = MockRoleManager<IdentityRole>();
        identityOptions = identityOptions ?? new IdentityOptions();
        var options = new Mock<IOptions<IdentityOptions>>();
        options.Setup(a => a.Value).Returns(identityOptions);
        var claimsFactory = new UserClaimsPrincipalFactory<IdentityUser, IdentityRole>(manager, roleManager.Object, options.Object);
        schemeProvider = schemeProvider ?? new Mock<IAuthenticationSchemeProvider>().Object;
        var sm = new Mock<SignInManager<IdentityUser>>(manager, contextAccessor.Object, claimsFactory, options.Object, logger ?? NullLogger<SignInManager<IdentityUser>>.Instance, schemeProvider, new DefaultUserConfirmation<IdentityUser>());
        return sm;
    }

    public static Mock<IUrlHelper> CreateMockUrlHelper(ActionContext context = null)
    {
        context ??= GetActionContextForPage("/Page");

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.SetupGet(h => h.ActionContext)
            .Returns(context);
        return urlHelper;
    }

    private static ActionContext GetActionContextForPage(string page)
    {
        // Create HttpContext mock
        var requestUrl = new Uri("http://myrequesturl");
        var request = Mock.Of<HttpRequest>();
        var requestMock = Mock.Get(request);
        var httpcontext = Mock.Of<HttpContext>();
        var httpcontextSetup = Mock.Get(httpcontext);
        httpcontextSetup.Setup(m => m.Request).Returns(request);

        return new()
        {
            ActionDescriptor = new()
            {
                RouteValues = new Dictionary<string, string?>
                {
                    { "page", page },
                }
            },
            RouteData = new()
            {
                Values =
                {
                    [ "page" ] = page
                }
            },
            HttpContext = httpcontext
        };
    }
}
