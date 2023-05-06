using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels;
using System.Text.Json;

namespace OceansAppWeb.Controllers
{
    [Authorize]
    [RequireTwoFactorEnabled]
    public class HomeController : Controller
   
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(UserManager<IdentityUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
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

        [HttpGet]
        public async Task<IActionResult> GetProvidersGroupByCategory()
        {
            var providerList = await _unitOfWork.Provider.GetProvidersGroupByCategoryAsync("S");
            string jsonResult = JsonSerializer.Serialize(providerList);
            return Content(jsonResult, "application/json");
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
