using OceansApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OceansApp.Areas.Admin.Controllers
{
    [Area("AdminCenter")]
    [Authorize(Roles = SD.Role_User_Master)]
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
