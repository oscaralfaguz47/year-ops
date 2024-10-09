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

            var userConsultant = await _unitOfWork.ApplicationUser.GetUserAndConsultantAsync(userId);

            DateTime? startDate = userConsultant.UserCategoryName == "External User" ? null : userConsultant.StartDate;

            var widgets = await _unitOfWork.ApplicationUser.GetWidgetsForUserAsync(userConsultant, User);

            var viewModel = new DashboardVM
            {
                Welcome = new(){ ConsultantName = userConsultant.Name, StartDate = startDate},
                Widgets = widgets 
            };
            return View("Dashboard/Dashboard", viewModel);
        }

        [HttpGet("GetPendingTimesheets")]
        public async Task<IActionResult> GetPendingTimesheets()
        {
            try
            {
                

                return Ok(new
                {
                    pendingTimesheets = "Hola amigos"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue."});
            }
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
