using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OceansApp.Areas.Admin.Controllers
{
    [Area("AdminCenter")]
    [Authorize(Policy = "AnyOfPoliciesInAdminCenter")]
    [RequireTwoFactorEnabled]
    public class AdminCenterController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View("Home");
        }
    }
}
