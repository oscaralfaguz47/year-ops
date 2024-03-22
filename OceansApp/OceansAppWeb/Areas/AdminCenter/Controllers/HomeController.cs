using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace OceansApp.Areas.AdminCenter.Controllers
{
    [Area("AdminCenter")]
    [EnableCors("AllowSpecificOrigin")]
    [RequireTwoFactorEnabled]
    [Authorize]
    [Authorize(Policy = "AnyOfPoliciesInAdminCenter")]
    public class HomeController : Controller
    {
        public async Task<IActionResult> IndexAsync()
        {
            return View();
        }
    }
}
