using Microsoft.AspNetCore.Mvc;

namespace OceansAppWeb.Areas.Finances.Controllers
{
    public class CalculatorController : Controller
    {
        [Area("Finances")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
