using Microsoft.AspNetCore.Mvc;

namespace OceansAppWeb.Areas.ProjectManagement.Controllers
{
    [Area("ProjectManagement")]
    [RequireTwoFactorEnabled]
    public class ClientsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
