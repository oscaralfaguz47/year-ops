using OceansApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OceansApp.Areas.AdminCenter.Controllers
{
    [Area("AdminCenter")]
    [Authorize(Roles = SD.Role_User_Master)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
