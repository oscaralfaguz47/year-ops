using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OceansApp.Areas.AdminCenter.Controllers
{
    [Area("AdminCenter")]
    [RequireTwoFactorEnabled]
    [Authorize(Policy = "AnyOfPoliciesInAdminCenter")]
    public class HomeController : Controller
    {
        public async Task<IActionResult> IndexAsync()
        {
            return View();
        }
    }
}
