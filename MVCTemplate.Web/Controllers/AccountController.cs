using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MVCTemplate.Web.Controllers
{
    public class AccountController : Controller
    {
        public AccountController()
        {
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
    }
}
