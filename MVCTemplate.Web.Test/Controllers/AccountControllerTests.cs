using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MVCTemplate.Web.Controllers;
using MVCTemplate.Web.Models;
using MVCTemplate.Web.Services;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace MVCTemplate.Web.Test.Controllers
{
    [TestClass]
    public class AccountControllerTests
    {
        private static Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
        private static Mock<UserManager<IdentityUser>> userManagerMock = MockHelper.MockUserManager<IdentityUser>();
        private static SignInManager<IdentityUser> signInManager = MockHelper.SetupSignInManager(userManagerMock.Object, httpContextMock.Object);
        IEmailService emailService = (new Mock<IEmailService>()).Object;

        [TestMethod]
        public void LoginGetTest()
        {

            AccountController accountController = new AccountController(signInManager, userManagerMock.Object, emailService);

            var result = accountController.Login();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void LoginSucceedTest()
        {
            Mock<SignInManager<IdentityUser>> signInManagerMock = MockHelper.SetupSignInManagerMock(userManagerMock.Object, httpContextMock.Object);
            signInManagerMock.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false)).Returns(
                Task.FromResult(SignInResult.Success));
            AccountController accountController = new AccountController(signInManagerMock.Object, userManagerMock.Object, emailService);

            var result = accountController.Login(new Models.CredentialViewModel
            {
                Email = "test@test.com",
                Password = "abc123"
            }).Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var actionResult = (RedirectToActionResult)result;
            Assert.AreEqual("Home", actionResult.ControllerName);
            Assert.AreEqual("Index", actionResult.ActionName);
        }

        [TestMethod]
        public void LoginIsLockedOut()
        {
            Mock<SignInManager<IdentityUser>> signInManagerMock = MockHelper.SetupSignInManagerMock(userManagerMock.Object, httpContextMock.Object);
            signInManagerMock.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false)).Returns(
                Task.FromResult(SignInResult.LockedOut));
            AccountController accountController = new AccountController(signInManagerMock.Object, userManagerMock.Object, emailService);

            var result = accountController.Login(new Models.CredentialViewModel
            {
                Email = "test@test.com",
                Password = "abc123"
            }).Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.ViewData.ModelState["Login"]);
            Assert.IsNotNull(viewResult.ViewData.ModelState["Login"].Errors.FirstOrDefault());
            Assert.AreEqual("You are locked out", viewResult.ViewData.ModelState["Login"].Errors.FirstOrDefault().ErrorMessage);
        }

        [TestMethod]
        public void LoginFailed()
        {
            Mock<SignInManager<IdentityUser>> signInManagerMock = MockHelper.SetupSignInManagerMock(userManagerMock.Object, httpContextMock.Object);
            signInManagerMock.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false)).Returns(
                Task.FromResult(SignInResult.Failed));
            AccountController accountController = new AccountController(signInManagerMock.Object, userManagerMock.Object, emailService);

            var result = accountController.Login(new Models.CredentialViewModel
            {
                Email = "test@test.com",
                Password = "abc123"
            }).Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.ViewData.ModelState["Login"]);
            Assert.IsNotNull(viewResult.ViewData.ModelState["Login"].Errors.FirstOrDefault());
            Assert.AreEqual("Failed to login", viewResult.ViewData.ModelState["Login"].Errors.FirstOrDefault().ErrorMessage);

        }

        [TestMethod]
        public void RegisterGet()
        {
            AccountController accountController = new AccountController(signInManager, userManagerMock.Object, emailService);

            var result = accountController.Register();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void RegisterSucceed()
        {
            var mockUrlHelper = MockHelper.CreateMockUrlHelper();
            Mock<UserManager<IdentityUser>> mockUserManager = MockHelper.MockUserManager<IdentityUser>();
            mockUserManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(),It.IsAny<string>())).Returns(
                Task.FromResult(IdentityResult.Success));
            mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<IdentityUser>())).Returns(
                Task.FromResult("aaa"));
            Mock<IEmailService> mockEmailServce = new Mock<IEmailService>();
            mockEmailServce.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(
                Task.FromResult(new { }));
            AccountController accountController = new AccountController(signInManager, mockUserManager.Object, mockEmailServce.Object);
            accountController.Url = mockUrlHelper.Object;

            var result = accountController.Register(new Models.RegisterViewModel
            {
                Email = "test@test.com",
                Password = "abc123"
            }).Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var actionResult = (RedirectToActionResult)result;
            Assert.AreEqual("Account", actionResult.ControllerName);
            Assert.AreEqual("Login", actionResult.ActionName);
        }

        [TestMethod]
        public void RegisterFailed()
        {
            Mock<UserManager<IdentityUser>> mockUserManager = MockHelper.MockUserManager<IdentityUser>();
            mockUserManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).Returns(
                Task.FromResult(IdentityResult.Failed(new IdentityError[]
                {
                    new IdentityError
                    {
                        Code = "Register",
                        Description = "Invalid input"
                    }
                })));
            AccountController accountController = new AccountController(signInManager, mockUserManager.Object, emailService);

            var result = accountController.Register(new Models.RegisterViewModel
            {
                Email = "test@test.com",
                Password = "abc123"
            }).Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.ViewData.ModelState["Register"]);
            Assert.IsNotNull(viewResult.ViewData.ModelState["Register"].Errors.FirstOrDefault());
            Assert.AreEqual("Invalid input", viewResult.ViewData.ModelState["Register"].Errors.FirstOrDefault().ErrorMessage);
        }

        [TestMethod]
        public void ConfirmEmailSucceed()
        {
            Mock<UserManager<IdentityUser>> mockUserManager = MockHelper.MockUserManager<IdentityUser>();
            mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).Returns(
                Task.FromResult(new IdentityUser
                {
                    Id = "123"
                }));
            mockUserManager.Setup(x => x.ConfirmEmailAsync(It.IsAny<IdentityUser>(),It.IsAny<string>())).Returns(
                Task.FromResult(IdentityResult.Success));
            AccountController accountController = new AccountController(signInManager, mockUserManager.Object, emailService);

            var result = accountController.ConfirmEmail("test@test.com","abc123").Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model);
            Assert.IsInstanceOfType(viewResult.Model, typeof(ConfirmEmailViewModel));
            var viewModel = (ConfirmEmailViewModel)viewResult.Model;
            Assert.AreEqual("Email address successfully confirmed", viewModel.Message);
        }

        [TestMethod]
        public void ConfirmEmailFail()
        {
            Mock<UserManager<IdentityUser>> mockUserManager = MockHelper.MockUserManager<IdentityUser>();
            mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).Returns(
                Task.FromResult(new IdentityUser
                {
                    Id = "123"
                }));
            mockUserManager.Setup(x => x.ConfirmEmailAsync(It.IsAny<IdentityUser>(),It.IsAny<string>())).Returns(
                Task.FromResult(IdentityResult.Failed(new IdentityError[]
                {
                    new IdentityError
                    {
                        Code = "ConfirmEmail",
                        Description = "Failed to confirm"
                    }
                })));
            AccountController accountController = new AccountController(signInManager, mockUserManager.Object, emailService);

            var result = accountController.ConfirmEmail("test@test.com","abc123").Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model);
            Assert.IsInstanceOfType(viewResult.Model, typeof(ConfirmEmailViewModel));
            var viewModel = (ConfirmEmailViewModel)viewResult.Model;
            Assert.AreEqual("Failed to validate email", viewModel.Message);
        }

        [TestMethod]
        public void ConfirmEmailNullUserFound()
        {
            Mock<UserManager<IdentityUser>> mockUserManager = MockHelper.MockUserManager<IdentityUser>();
            AccountController accountController = new AccountController(signInManager, mockUserManager.Object, emailService);

            var result = accountController.ConfirmEmail("test@test.com","abc123").Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model);
            Assert.IsInstanceOfType(viewResult.Model, typeof(ConfirmEmailViewModel));
            var viewModel = (ConfirmEmailViewModel)viewResult.Model;
            Assert.AreEqual("Failed to validate email", viewModel.Message);
        }
    }
}