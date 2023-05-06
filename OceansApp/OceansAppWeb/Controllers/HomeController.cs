using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels;

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

        public async Task<IActionResult> Dashboard()
        {
            var providersGroupByCategory = await _unitOfWork.Provider.GetProvidersGroupByCategoryAsync("S");

            var viewModel = new DashboardVM
            {
                ProvidersGroupByCategory = providersGroupByCategory
            };
            return View(viewModel);
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
