using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.WeeklyPulse;

namespace OceansApp.Areas.WeeklyPulse.Controllers
{
    /// <summary>
    /// Weekly Pulse Settings — the Administer-gated home for the structural records:
    /// Teams and their KPI definitions. Everything here is a <b>guarded</b> mutation: the
    /// views require an explicit client-side confirm before the POST, because these records
    /// reshape what the meeting expects (unlike the frictionless everyday Dashboard edits).
    /// </summary>
    [Area("WeeklyPulse")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    [Authorize]
    [Authorize(Policy = "Administer")]
    public class SettingsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public SettingsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var teams = await _unitOfWork.Team.GetAllAsync(
                orderBy: q => q.OrderBy(t => t.DisplayOrder));

            var kpis = await _unitOfWork.KpiDefinition.GetAllAsync();
            var people = await _unitOfWork.ApplicationUser.GetAllAsync();

            var vm = new SettingsVM
            {
                Teams = teams.Select(t => new TeamSettingsVM
                {
                    Team = t,
                    Kpis = kpis.Where(k => k.TeamId == t.TeamId)
                               .OrderBy(k => k.Name)
                               .ToList()
                }).ToList(),
                People = people
                    .OrderBy(p => p.Name).ThenBy(p => p.LastName)
                    .Select(p => new PersonOptionVM
                    {
                        Id = p.Id,
                        DisplayName = $"{p.Name} {p.LastName}".Trim()
                    })
                    .ToList()
            };

            return View(vm);
        }

        // ---- Teams (guarded) ------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeam(string name, string teamLeaderId, int displayOrder)
        {
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(teamLeaderId))
            {
                // A Person may lead more than one Team — no uniqueness check on the leader.
                await _unitOfWork.Team.AddAsync(new Team
                {
                    Name = name,
                    TeamLeaderId = teamLeaderId,
                    DisplayOrder = displayOrder
                });
                await _unitOfWork.SaveAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeam(int teamId, string name, string teamLeaderId, int displayOrder)
        {
            var team = await _unitOfWork.Team.GetFirstOrDefaultAsync(t => t.TeamId == teamId);
            if (team != null && !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(teamLeaderId))
            {
                team.Name = name;
                team.TeamLeaderId = teamLeaderId;
                team.DisplayOrder = displayOrder;
                _unitOfWork.Team.Update(team);
                await _unitOfWork.SaveAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ---- KPI definitions (guarded) -------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateKpi(int teamId, string name, string ownerId, string target, bool inScope)
        {
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(ownerId) && !string.IsNullOrWhiteSpace(target))
            {
                await _unitOfWork.KpiDefinition.AddAsync(new KpiDefinition
                {
                    TeamId = teamId,
                    Name = name,
                    OwnerId = ownerId,
                    Target = target,
                    Active = true,        // created live
                    InScope = inScope     // scope is independent of live/retired
                });
                await _unitOfWork.SaveAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditKpi(int kpiDefinitionId, string name, string ownerId, string target, bool active, bool inScope)
        {
            var kpi = await _unitOfWork.KpiDefinition.GetFirstOrDefaultAsync(k => k.KpiDefinitionId == kpiDefinitionId);
            if (kpi != null && !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(ownerId) && !string.IsNullOrWhiteSpace(target))
            {
                kpi.Name = name;
                kpi.OwnerId = ownerId;
                kpi.Target = target;
                // Active and InScope are two independent flags — set each from its own input.
                kpi.Active = active;
                kpi.InScope = inScope;
                _unitOfWork.KpiDefinition.Update(kpi);
                await _unitOfWork.SaveAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RetireKpi(int kpiDefinitionId)
        {
            // Retiring keeps the definition and its historical results; it just stops
            // expecting new input (drops from Readiness and the Review).
            await _unitOfWork.KpiDefinition.RetireAsync(kpiDefinitionId);
            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
