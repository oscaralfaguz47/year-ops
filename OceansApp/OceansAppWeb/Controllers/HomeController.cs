using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Dashboard;
using System.Security.Claims;

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
            return RedirectToAction("Dashboard/Dashboard");
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var applicationUser = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(x => x.Id == userId);
            if (applicationUser == null) return NotFound("The user was not found");

            var consultantDetail = await _unitOfWork.ConsultantDetail.GetFirstOrDefaultAsync(x => x.UserId == applicationUser.Id);

            DateTime? startDate = consultantDetail == null ? null : consultantDetail.StartDate;

            var widgets = await _unitOfWork.ApplicationUser.GetWidgetsForUserAsync(applicationUser, User);

            var viewModel = new DashboardVM
            {
                Welcome = new(){ ConsultantName = "Carlitos", StartDate = startDate},
                Widgets = widgets 
            };
            return View("Dashboard/Dashboard", viewModel);
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
