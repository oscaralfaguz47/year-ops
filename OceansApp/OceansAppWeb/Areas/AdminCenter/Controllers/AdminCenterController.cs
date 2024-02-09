using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace OceansApp.Areas.Admin.Controllers
{
    [Area("AdminCenter")]
    [EnableCors("AllowSpecificOrigin")]
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
