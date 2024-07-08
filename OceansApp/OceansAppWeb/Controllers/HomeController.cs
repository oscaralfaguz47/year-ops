using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using System.Text.Json;

namespace OceansAppWeb.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    public class HomeController : Controller
   
    {
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        public IActionResult Dashboard()
        {
            return View();
        }
        [Authorize(Policy = "AccessToConsultantsPage")]
        [HttpGet]
        public async Task<IActionResult> GetProvidersGroupByCategory()
        {
            var providerList = await _unitOfWork.Provider.GetProvidersGroupByCategoryAsync("S");
            string jsonResult = JsonSerializer.Serialize(providerList);
            return Content(jsonResult, "application/json");
        }
        public IActionResult Error()
        {
            return View("Error");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
