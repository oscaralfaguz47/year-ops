using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.WeeklyPulse;
using OceansApp.Utility.WeeklyPulse;

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
            var weekStart = WeekStartCalculator.Current();

            var teams = await _unitOfWork.Team.GetAllAsync(
                orderBy: q => q.OrderBy(t => t.DisplayOrder));

            // Snapshot: only this Week's check-ins, so a new Week starts blank.
            var checkIns = await _unitOfWork.CheckIn.GetAllAsync(
                filter: c => c.WeekStart == weekStart);

            var vm = new DashboardVM
            {
                WeekStart = weekStart,
                Teams = teams.Select(t => new TeamCheckInVM
                {
                    Team = t,
                    CheckIn = checkIns.FirstOrDefault(c => c.TeamId == t.TeamId)
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordCheckIn(int teamId, CheckInType type, string note)
        {
            await _unitOfWork.CheckIn.UpsertAsync(new CheckIn
            {
                TeamId = teamId,
                WeekStart = WeekStartCalculator.Current(),
                Type = type,
                Note = note
            });
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
