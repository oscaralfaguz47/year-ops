using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.WeeklyPulse;

namespace OceansApp.Areas.WeeklyPulse.Controllers
{
    /// <summary>
    /// Weekly Pulse Meeting History — past Weeks read back as minutes. There is no Week
    /// table (ADR 0001): the set of past Weeks is derived from the distinct WeekStart values
    /// across the data (see <see cref="MeetingHistoryService.DistinctWeeks"/>). Each Week
    /// shows its snapshots plus every living Issue/To-Do that was <b>active that Week</b>,
    /// shown <i>as of</i> that Week — active being a pure derivation (surfaced-in-Review OR
    /// commented/status-changed that Week), never stored. Browsing is Participate; deleting a
    /// past Week from the record is Administer.
    /// </summary>
    [Area("WeeklyPulse")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    [Authorize]
    [Authorize(Policy = "Participate")]
    public class HistoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public HistoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new MeetingHistoryVM { Weeks = (await DistinctWeeksAsync()).ToList() };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Week(DateOnly weekStart)
        {
            var teams = await _unitOfWork.Team.GetAllAsync(
                orderBy: q => q.OrderBy(t => t.DisplayOrder));

            // Snapshots stamped to this Week.
            var checkIns = await _unitOfWork.CheckIn.GetAllAsync(filter: c => c.WeekStart == weekStart);
            var headlines = (await _unitOfWork.Headline.GetForWeekAsync(weekStart)).ToList();
            var kpis = (await _unitOfWork.KpiDefinition.GetAllAsync()).ToList();
            var kpiResults = await _unitOfWork.KpiResult.GetAllAsync(filter: r => r.WeekStart == weekStart);

            // Living entities — read as of this Week, filtered to those active that Week.
            var issues = await _unitOfWork.Issue.GetAllAsync(includeProperties: nameof(Issue.History));
            var toDos = await _unitOfWork.ToDo.GetAllAsync(
                includeProperties: $"{nameof(ToDo.History)},{nameof(ToDo.Owner)}");

            var vm = new WeekMinutesVM
            {
                WeekStart = weekStart,
                Teams = teams.Select(t => new WeekMinutesTeamVM
                {
                    Team = t,
                    CheckIn = checkIns.FirstOrDefault(c => c.TeamId == t.TeamId),
                    Headlines = headlines.Where(h => h.TeamId == t.TeamId).ToList(),
                    Kpis = kpiResults
                        .Where(r => kpis.Any(k => k.KpiDefinitionId == r.KpiDefinitionId && k.TeamId == t.TeamId))
                        .Select(r => new KpiResultRowVM
                        {
                            Kpi = kpis.First(k => k.KpiDefinitionId == r.KpiDefinitionId),
                            Result = r
                        })
                        .OrderBy(r => r.Kpi.Name)
                        .ToList(),
                    // Each Issue active that Week, shown as of that Week (a single Issue
                    // appears under every Week it touched, with its per-week progression).
                    Issues = issues
                        .Where(i => i.TeamId == t.TeamId)
                        .Where(i => MeetingHistoryService.IssueActiveInWeek(i.History, i.Pinned, weekStart))
                        .Select(i => new IssueRowVM
                        {
                            Issue = i,
                            State = IssueStateService.StateAsOf(i.History, weekStart)
                        })
                        .OrderBy(r => r.State)
                        .ThenByDescending(r => r.Issue.Priority)
                        .ToList(),
                    ToDos = toDos
                        .Where(td => td.TeamId == t.TeamId)
                        .Where(td => MeetingHistoryService.ToDoActiveInWeek(td.History, weekStart))
                        .Select(td => new ToDoRowVM
                        {
                            ToDo = td,
                            State = ToDoStateService.StateAsOf(td.History, weekStart)
                        })
                        .OrderBy(r => r.State)
                        .ThenBy(r => r.ToDo.DueDate)
                        .ToList()
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administer")]
        public async Task<IActionResult> DeleteWeek(DateOnly weekStart)
        {
            // Administer-gated: remove a past Week from the record. Deletes everything stamped
            // to that WeekStart — the snapshots (check-ins, headlines, KPI results) and the
            // week-stamped living-entity history rows. The Issues/To-Dos themselves remain;
            // their state simply re-derives from whatever rows are left. One SaveAsync commits
            // it all atomically.
            var checkIns = await _unitOfWork.CheckIn.GetAllAsync(filter: c => c.WeekStart == weekStart);
            _unitOfWork.CheckIn.RemoveRange(checkIns);

            var headlines = await _unitOfWork.Headline.GetAllAsync(filter: h => h.WeekStart == weekStart);
            _unitOfWork.Headline.RemoveRange(headlines);

            var kpiResults = await _unitOfWork.KpiResult.GetAllAsync(filter: r => r.WeekStart == weekStart);
            _unitOfWork.KpiResult.RemoveRange(kpiResults);

            await _unitOfWork.Issue.DeleteHistoryForWeekAsync(weekStart);
            await _unitOfWork.ToDo.DeleteHistoryForWeekAsync(weekStart);

            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// The set of past Weeks = the distinct WeekStart values across every week-stamped
        /// source, newest first (see <see cref="MeetingHistoryService.DistinctWeeks"/>).
        /// </summary>
        private async Task<IReadOnlyList<DateOnly>> DistinctWeeksAsync()
        {
            var checkIns = await _unitOfWork.CheckIn.GetAllAsync();
            var kpiResults = await _unitOfWork.KpiResult.GetAllAsync();
            var headlines = await _unitOfWork.Headline.GetAllAsync();
            var issues = await _unitOfWork.Issue.GetAllAsync(includeProperties: nameof(Issue.History));
            var toDos = await _unitOfWork.ToDo.GetAllAsync(includeProperties: nameof(ToDo.History));

            var stamps = checkIns.Select(c => c.WeekStart)
                .Concat(kpiResults.Select(r => r.WeekStart))
                .Concat(headlines.Select(h => h.WeekStart))
                .Concat(issues.SelectMany(i => i.History).Select(h => h.WeekStart))
                .Concat(toDos.SelectMany(td => td.History).Select(h => h.WeekStart));

            return MeetingHistoryService.DistinctWeeks(stamps);
        }
    }
}
