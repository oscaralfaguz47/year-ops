using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Domain.WeeklyPulse;
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

        /// <summary>
        /// Bounce back to the Dashboard after a POST, preserving the bookmarkable
        /// <c>?teams=</c> filter the user posted from. The value is read off the Referer's
        /// query (not by redirecting to the Referer URL itself, which would be an
        /// open-redirect risk) and re-attached to a known-safe Index route. When the Referer
        /// is absent or carries no filter (e.g. stripped by a proxy, or posted from Review),
        /// it falls back to the unfiltered Dashboard.
        /// </summary>
        private IActionResult RedirectToDashboard()
        {
            var referer = Request.Headers.Referer.ToString();
            if (!string.IsNullOrEmpty(referer)
                && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri)
                && QueryHelpers.ParseQuery(refererUri.Query).TryGetValue("teams", out var teams)
                && !string.IsNullOrWhiteSpace(teams))
            {
                return RedirectToAction(nameof(Index), new { teams = teams.ToString() });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            [FromQuery(Name = "teams")] string? teamFilter = null)
        {
            var weekStart = WeekStartCalculator.Current();

            var teams = await _unitOfWork.Team.GetAllAsync(
                orderBy: q => q.OrderBy(t => t.DisplayOrder));

            // The ?teams= filter is bookmarkable: parse the CSV of TeamIds, keep only ids
            // that still exist (a stale bookmark to a deleted team is ignored), and default
            // to every team when absent or empty.
            var allTeamIds = teams.Select(t => t.TeamId).ToHashSet();
            var selectedTeamIds = (teamFilter ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var id) ? id : (int?)null)
                .Where(id => id.HasValue && allTeamIds.Contains(id.Value))
                .Select(id => id!.Value)
                .ToHashSet();
            if (selectedTeamIds.Count == 0)
            {
                selectedTeamIds = allTeamIds;
            }

            // Living: all Issues carry across Weeks; show each as of this Week.
            var issues = await _unitOfWork.Issue.GetAllAsync(
                includeProperties: nameof(Issue.History));

            // Living: all To-Dos carry across Weeks; show each as of this Week (non-Done only).
            var toDos = await _unitOfWork.ToDo.GetAllAsync(
                includeProperties: $"{nameof(ToDo.History)},{nameof(ToDo.Owner)}");

            // Candidate owners for the To-Do owner dropdown.
            var people = await _unitOfWork.ApplicationUser.GetAllAsync();

            // KPIs are structural (carry across Weeks); their results are this Week's snapshot.
            var kpis = (await _unitOfWork.KpiDefinition.GetAllAsync()).ToList();
            var kpiResults = await _unitOfWork.KpiResult.GetAllAsync(
                filter: r => r.WeekStart == weekStart);

            // Snapshot: only this Week's headlines, so a new Week starts blank.
            var headlines = (await _unitOfWork.Headline.GetForWeekAsync(weekStart)).ToList();

            var vm = new DashboardVM
            {
                WeekStart = weekStart,
                SelectedTeamIds = selectedTeamIds,
                Teams = teams.Select(t =>
                {
                    var teamKpis = kpis.Where(k => k.TeamId == t.TeamId).ToList();
                    var teamKpiIds = teamKpis.Select(k => k.KpiDefinitionId).ToHashSet();
                    var teamResults = kpiResults.Where(r => teamKpiIds.Contains(r.KpiDefinitionId)).ToList();

                    return new TeamCheckInVM
                    {
                        Team = t,
                        Issues = issues
                            .Where(i => i.TeamId == t.TeamId)
                            .Select(i => new IssueRowVM
                            {
                                Issue = i,
                                State = IssueStateService.StateAsOf(i.History, weekStart)
                            })
                            .OrderBy(r => r.State)
                            .ThenByDescending(r => r.Issue.Priority)
                            .ToList(),
                        // Readiness is computed only from KPIs (in meeting scope); it filters internally.
                        Readiness = ReadinessService.Evaluate(teamKpis, teamResults),
                        // Only KPIs that still expect input prompt for a result this Week.
                        Kpis = teamKpis
                            .Where(KpiScopeService.ExpectsInput)
                            .OrderBy(k => k.Name)
                            .Select(k => new KpiResultRowVM
                            {
                                Kpi = k,
                                Result = teamResults.FirstOrDefault(r => r.KpiDefinitionId == k.KpiDefinitionId)
                            })
                            .ToList(),
                        Headlines = headlines
                            .Where(h => h.TeamId == t.TeamId)
                            .ToList(),
                        // Living To-Dos surface on the Dashboard until Done.
                        ToDos = toDos
                            .Where(td => td.TeamId == t.TeamId)
                            .Select(td => new ToDoRowVM
                            {
                                ToDo = td,
                                State = ToDoStateService.StateAsOf(td.History, weekStart)
                            })
                            .Where(r => ReviewSurfacingService.ToDoShowsOnDashboard(r.State))
                            .OrderBy(r => r.State)
                            .ThenBy(r => r.ToDo.DueDate)
                            .ToList()
                    };
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RaiseIssue(int teamId, string title, IssuePriority priority)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                await _unitOfWork.Issue.RaiseAsync(new Issue
                {
                    TeamId = teamId,
                    Title = title,
                    Priority = priority,
                    OriginWeekStart = WeekStartCalculator.Current()
                }, DateTimeOffset.UtcNow);
                await _unitOfWork.SaveAsync();
            }

            return RedirectToDashboard();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CommentIssue(int issueId, string comment)
        {
            if (!string.IsNullOrWhiteSpace(comment))
            {
                await _unitOfWork.Issue.CommentAsync(
                    issueId, comment, WeekStartCalculator.Current(), DateTimeOffset.UtcNow);
                await _unitOfWork.SaveAsync();
            }

            return RedirectToDashboard();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransitionIssue(int issueId, IssueStatus status)
        {
            await _unitOfWork.Issue.TransitionAsync(
                issueId, status, WeekStartCalculator.Current(), DateTimeOffset.UtcNow);
            await _unitOfWork.SaveAsync();

            return RedirectToDashboard();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditIssue(int issueId, int teamId, string title, IssuePriority priority, IssueStatus status)
        {
            // Click-to-edit: the Issue's own fields (Team/title/priority) plus its derived
            // status, applied in one save. A required, non-blank title guards the edit.
            if (!string.IsNullOrWhiteSpace(title))
            {
                await _unitOfWork.Issue.EditAsync(
                    issueId, teamId, title, priority, status,
                    WeekStartCalculator.Current(), DateTimeOffset.UtcNow);
                await _unitOfWork.SaveAsync();
            }

            return RedirectToDashboard();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RaiseToDo(int teamId, string title, string ownerId, DateOnly dueDate)
        {
            if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(ownerId))
            {
                await _unitOfWork.ToDo.RaiseAsync(new ToDo
                {
                    TeamId = teamId,
                    Title = title,
                    OwnerId = ownerId,
                    DueDate = dueDate,
                    OriginWeekStart = WeekStartCalculator.Current()
                }, DateTimeOffset.UtcNow);
                await _unitOfWork.SaveAsync();
            }

            return RedirectToDashboard();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransitionToDo(int toDoId, ToDoStatus status)
        {
            await _unitOfWork.ToDo.TransitionAsync(
                toDoId, status, WeekStartCalculator.Current(), DateTimeOffset.UtcNow);
            await _unitOfWork.SaveAsync();

            return RedirectToDashboard();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditToDo(int toDoId, int teamId, string title, string ownerId, DateOnly dueDate)
        {
            // Click-to-edit: the To-Do's own fields (Team/owner/title/due date). Status is its
            // own lifecycle (TransitionToDo) and is not part of this edit. A required, non-blank
            // title and owner guard the edit.
            if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(ownerId))
            {
                await _unitOfWork.ToDo.EditAsync(toDoId, teamId, title, ownerId, dueDate);
                await _unitOfWork.SaveAsync();
            }

            return RedirectToDashboard();
        }

        [HttpGet]
        public async Task<IActionResult> Review()
        {
            var weekStart = WeekStartCalculator.Current();

            var teams = await _unitOfWork.Team.GetAllAsync(
                orderBy: q => q.OrderBy(t => t.DisplayOrder));

            var issues = await _unitOfWork.Issue.GetAllAsync(
                includeProperties: nameof(Issue.History));

            var toDos = await _unitOfWork.ToDo.GetAllAsync(
                includeProperties: $"{nameof(ToDo.History)},{nameof(ToDo.Owner)}");

            // Snapshot: this Week's headlines open each team's segment as a news round.
            var headlines = (await _unitOfWork.Headline.GetForWeekAsync(weekStart)).ToList();

            // KPIs are structural (carry across Weeks); their results are this Week's snapshot.
            var kpis = (await _unitOfWork.KpiDefinition.GetAllAsync()).ToList();
            var kpiResults = await _unitOfWork.KpiResult.GetAllAsync(
                filter: r => r.WeekStart == weekStart);

            // Surface the union of Open and pinned-Deferred issues, never Solved,
            // grouped by the record's Team in meeting order.
            var vm = new ReviewVM
            {
                WeekStart = weekStart,
                Teams = teams.Select(t => new ReviewTeamVM
                {
                    Team = t,
                    // KPIs: scope gate first (out-of-scope/retired drop), then Green-quiet.
                    Kpis = kpis
                        .Where(k => k.TeamId == t.TeamId)
                        .Select(k =>
                        {
                            var result = kpiResults.FirstOrDefault(r => r.KpiDefinitionId == k.KpiDefinitionId);
                            return new KpiReviewRowVM
                            {
                                Kpi = k,
                                Result = result,
                                Surfacing = ReviewSurfacingService.SurfaceKpi(k, result)
                            };
                        })
                        .Where(r => r.Surfacing != KpiSurfacing.Hidden)
                        // Loud (Red/Yellow/missing) first so concerns read first, then by name.
                        .OrderByDescending(r => r.Surfacing)
                        .ThenBy(r => r.Kpi.Name)
                        .ToList(),
                    // News round: every headline surfaces, Risk loud and Highlight quiet.
                    Headlines = headlines
                        .Where(h => h.TeamId == t.TeamId)
                        .Select(h => new HeadlineRowVM
                        {
                            Headline = h,
                            Emphasis = ReviewSurfacingService.SurfaceHeadline(h.Type)
                        })
                        .ToList(),
                    Issues = issues
                        .Where(i => i.TeamId == t.TeamId)
                        .Select(i => new IssueRowVM
                        {
                            Issue = i,
                            State = IssueStateService.StateAsOf(i.History, weekStart)
                        })
                        .Where(r => ReviewSurfacingService.Surfaces(r.State, r.Issue.Pinned))
                        .OrderByDescending(r => r.Issue.Priority)
                        .ToList(),
                    // Every non-Done To-Do surfaces; Blocked is flagged loud, Done is dropped.
                    ToDos = toDos
                        .Where(td => td.TeamId == t.TeamId)
                        .Select(td =>
                        {
                            var state = ToDoStateService.StateAsOf(td.History, weekStart);
                            return new ToDoRowVM
                            {
                                ToDo = td,
                                State = state,
                                Surfacing = ReviewSurfacingService.SurfaceToDo(state)
                            };
                        })
                        .Where(r => r.Surfacing != ToDoSurfacing.Hidden)
                        // Blocked (Loud) first so it reads loud, then by due date.
                        .OrderByDescending(r => r.Surfacing)
                        .ThenBy(r => r.ToDo.DueDate)
                        .ToList()
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostHeadline(int teamId, HeadlineType type, string text)
        {
            // Posting a headline is a frictionless everyday edit. Headlines are additive —
            // each post is its own row (there may be many per Team/Week, the news round).
            if (!string.IsNullOrWhiteSpace(text))
            {
                await _unitOfWork.Headline.PostAsync(new Headline
                {
                    TeamId = teamId,
                    WeekStart = WeekStartCalculator.Current(),
                    Type = type,
                    Text = text
                });
                await _unitOfWork.SaveAsync();
            }

            return RedirectToDashboard();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHeadline(int headlineId, int teamId, HeadlineType type, string text)
        {
            // Click-to-edit: a headline is a snapshot — its Team, type and text move, its Week
            // stays. A required, non-blank text guards the edit.
            if (!string.IsNullOrWhiteSpace(text))
            {
                await _unitOfWork.Headline.EditAsync(headlineId, teamId, type, text);
                await _unitOfWork.SaveAsync();
            }

            return RedirectToDashboard();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertHeadlineToIssue(int headlineId)
        {
            // Additive conversion: the headline stays in its Week; a new pre-filled Issue
            // is raised carrying a back-reference to it.
            await _unitOfWork.Issue.ConvertHeadlineAsync(
                headlineId, WeekStartCalculator.Current(), DateTimeOffset.UtcNow);
            await _unitOfWork.SaveAsync();

            return RedirectToDashboard();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertIssueToToDo(int issueId, string ownerId, DateOnly dueDate)
        {
            // Additive conversion: the source Issue stays intact; a new pre-filled To-Do is
            // raised carrying a back-reference to it. The To-Do's owner and due date (which
            // an Issue does not carry) come from the form.
            if (!string.IsNullOrWhiteSpace(ownerId))
            {
                await _unitOfWork.ToDo.ConvertIssueAsync(
                    issueId, ownerId, dueDate, WeekStartCalculator.Current(), DateTimeOffset.UtcNow);
                await _unitOfWork.SaveAsync();
            }

            return RedirectToDashboard();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPin(int issueId, bool pinned)
        {
            // The pin is offered only on Deferred issues; the repository rejects pinning
            // any other state at the model level.
            await _unitOfWork.Issue.SetPinAsync(issueId, pinned, WeekStartCalculator.Current());
            await _unitOfWork.SaveAsync();

            return RedirectToDashboard();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordKpiResult(int kpiDefinitionId, string value, KpiStatus status, string? notes)
        {
            // Recording a KPI result is a frictionless everyday edit (unlike the guarded
            // definition). Upsert keeps exactly one result per (KPI, Week).
            if (!string.IsNullOrWhiteSpace(value))
            {
                await _unitOfWork.KpiResult.UpsertAsync(new KpiResult
                {
                    KpiDefinitionId = kpiDefinitionId,
                    WeekStart = WeekStartCalculator.Current(),
                    Value = value,
                    Status = status,
                    Notes = notes
                });
                await _unitOfWork.SaveAsync();
            }

            return RedirectToDashboard();
        }
    }
}
