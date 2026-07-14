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
                        .Where(i => MeetingHistoryService.IssueActiveInWeek(i.History, i.OriginWeekStart, weekStart))
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
                        .Where(td => MeetingHistoryService.ToDoActiveInWeek(td.History, td.OriginWeekStart, weekStart))
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

        /// <summary>
        /// Weekly Summary — an auto-assembled, READ-ONLY draft computed on the fly from the
        /// Week's data (never stored — see ADR 0001). For each Team: decisions are the Issues
        /// Solved this Week, actions the To-Dos raised this Week, risks the Week's Risk-type
        /// Headlines plus its open High/Critical Issues, and a single suggested-format
        /// sentence (see <see cref="WeeklySummaryService.Derive"/>). Read-only (Participate);
        /// no WeeklySummary row is created.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Summary(DateOnly weekStart)
        {
            var teams = await _unitOfWork.Team.GetAllAsync(
                orderBy: q => q.OrderBy(t => t.DisplayOrder));

            var headlines = (await _unitOfWork.Headline.GetForWeekAsync(weekStart)).ToList();
            var issues = await _unitOfWork.Issue.GetAllAsync(includeProperties: nameof(Issue.History));
            var toDos = await _unitOfWork.ToDo.GetAllAsync(
                includeProperties: $"{nameof(ToDo.History)},{nameof(ToDo.Owner)}");

            var vm = new WeeklySummaryVM
            {
                WeekStart = weekStart,
                Teams = teams.Select(t => new WeeklySummaryTeamVM
                {
                    Team = t,
                    // Pure derivation over this Team's slice of the Week's data — nothing stored.
                    Summary = WeeklySummaryService.Derive(
                        issues.Where(i => i.TeamId == t.TeamId),
                        toDos.Where(td => td.TeamId == t.TeamId),
                        headlines.Where(h => h.TeamId == t.TeamId),
                        weekStart)
                }).ToList()
            };

            return View(vm);
        }

        /// <summary>
        /// KPI History picker — the KPI definitions whose weekly results can be read back as
        /// a Period-grouped history. Read-only (Participate).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Kpis()
        {
            var kpis = await _unitOfWork.KpiDefinition.GetAllAsync(
                includeProperties: nameof(KpiDefinition.Team));

            var vm = new KpiHistoryIndexVM
            {
                Kpis = kpis.OrderBy(k => k.Team?.DisplayOrder).ThenBy(k => k.Name).ToList()
            };
            return View(vm);
        }

        /// <summary>
        /// KPI History for a single KPI — its weekly results read in sequence and grouped by
        /// the selected Period granularity (month / quarter / year). Display only: a Week
        /// belongs wholly to the Period containing its Monday and no value is summed or
        /// averaged (see <see cref="KpiHistoryService.GroupByPeriod"/>).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Kpi(int kpiDefinitionId, PeriodGranularity granularity = PeriodGranularity.Month)
        {
            var kpi = (await _unitOfWork.KpiDefinition.GetAllAsync(
                filter: k => k.KpiDefinitionId == kpiDefinitionId,
                includeProperties: nameof(KpiDefinition.Team))).FirstOrDefault();
            if (kpi == null)
            {
                return NotFound();
            }

            var results = await _unitOfWork.KpiResult.GetAllAsync(
                filter: r => r.KpiDefinitionId == kpiDefinitionId);

            var vm = new KpiHistoryVM
            {
                Kpi = kpi,
                Granularity = granularity,
                Periods = KpiHistoryService.GroupByPeriod(results, granularity)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administer")]
        public async Task<IActionResult> DeleteWeek(DateOnly weekStart)
        {
            // Administer-gated: remove a past Week from the record. Deletes everything stamped
            // to that WeekStart — the snapshots (headlines, KPI results) and the
            // week-stamped living-entity history rows. An Issue/To-Do active in other Weeks
            // keeps its remaining rows and simply re-derives its state; but one whose ENTIRE
            // history was in this Week would be left with no rows and silently re-derive to a
            // phantom Open state forever (StateAsOf defaults to Open) — so those orphans are
            // removed outright. One SaveAsync commits it all atomically.
            var headlines = await _unitOfWork.Headline.GetAllAsync(filter: h => h.WeekStart == weekStart);
            _unitOfWork.Headline.RemoveRange(headlines);

            var kpiResults = await _unitOfWork.KpiResult.GetAllAsync(filter: r => r.WeekStart == weekStart);
            _unitOfWork.KpiResult.RemoveRange(kpiResults);

            await _unitOfWork.Issue.DeleteHistoryForWeekAsync(weekStart);
            await _unitOfWork.ToDo.DeleteHistoryForWeekAsync(weekStart);

            // Remove living entities whose only history was in the deleted Week.
            var issues = await _unitOfWork.Issue.GetAllAsync(includeProperties: nameof(Issue.History));
            _unitOfWork.Issue.RemoveRange(
                issues.Where(i => i.History.Any() && i.History.All(h => h.WeekStart == weekStart)).ToList());

            var toDos = await _unitOfWork.ToDo.GetAllAsync(includeProperties: nameof(ToDo.History));
            _unitOfWork.ToDo.RemoveRange(
                toDos.Where(td => td.History.Any() && td.History.All(h => h.WeekStart == weekStart)).ToList());

            await _unitOfWork.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// The set of past Weeks = the distinct WeekStart values across every week-stamped
        /// source, newest first (see <see cref="MeetingHistoryService.DistinctWeeks"/>).
        /// </summary>
        private async Task<IReadOnlyList<DateOnly>> DistinctWeeksAsync()
        {
            var kpiResults = await _unitOfWork.KpiResult.GetAllAsync();
            var headlines = await _unitOfWork.Headline.GetAllAsync();
            var issues = await _unitOfWork.Issue.GetAllAsync(includeProperties: nameof(Issue.History));
            var toDos = await _unitOfWork.ToDo.GetAllAsync(includeProperties: nameof(ToDo.History));

            var stamps = kpiResults.Select(r => r.WeekStart)
                .Concat(headlines.Select(h => h.WeekStart))
                .Concat(issues.SelectMany(i => i.History).Select(h => h.WeekStart))
                .Concat(toDos.SelectMany(td => td.History).Select(h => h.WeekStart));

            return MeetingHistoryService.DistinctWeeks(stamps);
        }
    }
}
