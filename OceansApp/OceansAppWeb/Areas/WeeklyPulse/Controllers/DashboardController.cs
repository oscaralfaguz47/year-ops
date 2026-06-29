using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;

namespace OceansApp.Areas.WeeklyPulse.Controllers
{
    [Area("WeeklyPulse")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    [Authorize]
    [Authorize(Policy = "Participate")]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var teams = await _unitOfWork.Team.GetAllAsync(
                orderBy: q => q.OrderBy(t => t.DisplayOrder));

            return View(teams);
        }
    }
}
