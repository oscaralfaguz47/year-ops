using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace OceansApp.Areas.Admin.Controllers
{
    [Area("AdminCenter")]
    [EnableCors("AllowSpecificOrigin")]
    [Authorize]
    [Authorize(Policy = "AnyOfPoliciesInAdminCenter")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    public class AdminCenterController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View("Home");
        }
    }
}
