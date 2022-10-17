using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MVCTemplate.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuthorizationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public AuthorizationController(RoleManager<IdentityRole> roleManager)
        {
            this.roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult ViewRoles()
        {
            var roles = roleManager.Roles;
            return View();
        }
    }
}
