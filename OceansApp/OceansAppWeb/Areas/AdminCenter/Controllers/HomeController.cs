using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OceansApp.Areas.AdminCenter.Controllers
{
    [Area("AdminCenter")]
    [Authorize(Policy = "AnyOfPoliciesInAdminCenter")]
    [RequireTwoFactorEnabled]
    public class HomeController : Controller
    {
        public async Task<IActionResult> IndexAsync()
        {
            return View();
        }
    }
}
