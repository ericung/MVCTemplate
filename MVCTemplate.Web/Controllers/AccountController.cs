using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MVCTemplate.Web.Models;
using System.Security.Cryptography.X509Certificates;

namespace MVCTemplate.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> signInManager;

        [BindProperty]
        public CredentialViewModel Credential { get; set; }

        public AccountController(SignInManager<IdentityUser> signInManager)
        {
            this.signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            Credential = new CredentialViewModel();
            return View(Credential);
        }

        [HttpPost]
        public async Task<IActionResult> Login([Bind("Email","Password","RememberMe")] CredentialViewModel credential)
        {
            if (!ModelState.IsValid) return View();

            var result = await this.signInManager.PasswordSignInAsync(
                this.Credential.Email,
                this.Credential.Password,
                this.Credential.RememberMe,
                false);

            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
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
    }
}
