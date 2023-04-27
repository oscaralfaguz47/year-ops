using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OceansAppWeb.Areas.Finances.Controllers
{
    [Area("Finances")]
    public class CalculatorController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
