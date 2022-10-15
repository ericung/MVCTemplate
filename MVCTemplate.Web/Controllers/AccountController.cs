using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MVCTemplate.Web.Models;
using MVCTemplate.Web.Services;
using System.Security.Cryptography.X509Certificates;

namespace MVCTemplate.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IEmailService emailService;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IEmailService emailService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([Bind("Email","Password","RememberMe")] CredentialViewModel credential)
        {
            if (!ModelState.IsValid) return View();

            var result = await this.signInManager.PasswordSignInAsync(
                credential.Email,
                credential.Password,
                credential.RememberMe,
                false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index","Home");
            }
            else
            {
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("Login", "You are locked out");
                }
                else
                {
                    ModelState.AddModelError("Login", "Failed to login");
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([Bind("Email","Password")] RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid) return View();

            var user = new IdentityUser
            {
                Email = registerViewModel.Email,
                UserName = registerViewModel.Email
            };

            var result = await userManager.CreateAsync(user, registerViewModel.Password);
            if (result.Succeeded)
            {
                var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.ActionLink(action: "ConfirmEmail", controller: "Account",
                    values: new { userId = user.Id, token = confirmationToken });
                   

                await emailService.SendAsync("ungericwei@gmail.com",
                    user.Email,
                    "Please confirm your email",
                    $"Please click on this link to confirm your email address: {confirmationLink}");

                return RedirectToAction("Login","Account");
            }
            else
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("Register", error.Description);
                }

                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user != null)
            {
                var result = await userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                {
                    return View(new ConfirmEmailViewModel { Message = "Email address successfully confirmed" });
                }
            }

            return View(new ConfirmEmailViewModel { Message = "Failed to validate email" });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
