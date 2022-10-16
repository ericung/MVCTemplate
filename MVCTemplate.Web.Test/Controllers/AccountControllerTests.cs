using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MVCTemplate.Web.Controllers;
using MVCTemplate.Web.Services;

namespace MVCTemplate.Web.Test.Controllers
{
    [TestClass]
    public class AccountControllerTests
    {
        private static Mock<UserManager<IdentityUser>> userManager = MockHelper.MockUserManager<IdentityUser>();
        private static SignInManager<IdentityUser> signInManager = MockHelper.SetupSignInManager(userManager.Object, new Mock<HttpContext>().Object);
        IEmailService emailService = (new Mock<IEmailService>()).Object;

        [TestMethod]
        public void LoginGetTest()
        {

            AccountController accountController = new AccountController(signInManager, userManager.Object, emailService);

            var result = accountController.Login();

            Assert.IsNotNull(result);
        }
    }
}