using OceansApp.Models.Models;

namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// Pure domain logic for the Weekly Pulse <b>Weekly Summary</b> — an auto-assembled,
    /// READ-ONLY draft computed on the fly from a Week's data. It is <b>never a stored
    /// entity</b> (there is no WeeklySummary table — see ADR 0001): the Summary is a plain
    /// derivation, re-computed each time it is read, so editing/override is out of scope
    /// for v1 (that would require persisting a record).
    ///
    /// <para>The draft assembles four parts, all from the Week's existing data:
    /// <list type="bullet">
    /// <item><b>Decisions</b> &lt;- Issues <i>Solved this Week</i> (the solving transition
    /// landed in this Week, per <see cref="SolvedInWeek"/>).</item>
    /// <item><b>Actions</b> &lt;- To-Dos <i>created in-meeting this Week</i>
    /// (<see cref="ToDo.OriginWeekStart"/> == the Week).</item>
    /// <item><b>Risks</b> &lt;- this Week's <see cref="HeadlineType.Risk"/> Headlines plus the
    /// High/Critical Issues that are still Open as of this Week.</item>
    /// <item><b>Summary text</b> &lt;- a single suggested-format sentence over the counts.</item>
    /// </list></para>
    ///
    /// Has <b>no EF Core / HttpContext dependency</b>: it operates purely on plain
    /// Issue/To-Do/Headline rows (with the living entities' history) plus the Week stamp, so it
    /// is fully unit-testable. Plain derivation, no AI. See ADR 0001 and CONTEXT.md.
    /// </summary>
    public static class WeeklySummaryService
    {
        /// <summary>
        /// Assemble the read-only Weekly Summary for <paramref name="week"/> from the Week's
        /// data. The inputs are the (already team-scoped) living Issues, living To-Dos and
        /// Headlines; the result is a fresh <see cref="WeeklySummary"/> — nothing is stored.
        /// </summary>
        public static WeeklySummary Derive(
            IEnumerable<Issue> issues,
            IEnumerable<ToDo> toDos,
            IEnumerable<Headline> headlines,
            DateOnly week)
        {
            var issueList = issues as ICollection<Issue> ?? issues.ToList();

            // Decisions: Issues whose solving transition landed in this Week.
            var decisions = issueList
                .Where(i => SolvedInWeek(i.History, week))
                .OrderByDescending(i => i.Priority)
                .ThenBy(i => i.Title)
                .ToList();

            // Actions: To-Dos raised in-meeting this Week.
            var actions = toDos
                .Where(td => td.OriginWeekStart == week)
                .OrderBy(td => td.DueDate)
                .ThenBy(td => td.Title)
                .ToList();

            // Risks, part 1: this Week's Risk-type Headlines.
            var riskHeadlines = headlines
                .Where(h => h.WeekStart == week && h.Type == HeadlineType.Risk)
                .ToList();

            // Risks, part 2: High/Critical Issues still Open as of this Week. The
            // OriginWeekStart guard keeps an Issue out of Weeks before it existed —
            // StateAsOf defaults to Open for any Week with no row on/before it, which would
            // otherwise flag a not-yet-created Issue as an open risk.
            var riskIssues = issueList
                .Where(i => i.OriginWeekStart <= week
                            && (i.Priority == IssuePriority.High || i.Priority == IssuePriority.Critical)
                            && IssueStateService.StateAsOf(i.History, week) == IssueStatus.Open)
                .OrderByDescending(i => i.Priority)
                .ThenBy(i => i.Title)
                .ToList();

            return new WeeklySummary
            {
                Week = week,
                Decisions = decisions,
                Actions = actions,
                RiskHeadlines = riskHeadlines,
                RiskIssues = riskIssues,
                SummaryText = ComposeSummaryText(
                    week, decisions.Count, actions.Count, riskHeadlines.Count + riskIssues.Count)
            };
        }

        /// <summary>
        /// Whether an Issue was <b>Solved in <paramref name="week"/></b>: the status row that
        /// determines its state as of <paramref name="week"/> (the latest Status row on or
        /// before it — same ordering as <see cref="IssueStateService.StateAsOf"/>) is itself
        /// stamped to that Week and is <see cref="IssueStatus.Solved"/>. So a Solve carried in
        /// from a prior Week does not count, and a Solve undone (re-opened) later in the same
        /// Week does not count either. Pure: derived from the history rows.
        /// </summary>
        public static bool SolvedInWeek(IEnumerable<IssueHistory> history, DateOnly week)
        {
            var determining = history
                .Where(h => h.ChangeType == IssueChangeType.Status && h.WeekStart <= week)
                .OrderBy(h => h.WeekStart)
                .ThenBy(h => h.CreatedAt)
                .ThenBy(h => h.IssueHistoryId)
                .LastOrDefault();

            return determining is { Status: IssueStatus.Solved } && determining.WeekStart == week;
        }

        /// <summary>The suggested-format summary sentence over the Week's tallies.</summary>
        private static string ComposeSummaryText(DateOnly week, int decisions, int actions, int risks) =>
            $"Week of {week:yyyy-MM-dd}: {decisions} decision(s) made, " +
            $"{actions} action(s) created, {risks} risk(s) flagged.";
    }

    /// <summary>
    /// The derived, read-only Weekly Summary draft for one Week (and one Team's slice of the
    /// data). Held only in memory — it is never persisted (see <see cref="WeeklySummaryService"/>).
    /// </summary>
    public class WeeklySummary
    {
        public DateOnly Week { get; init; }

        /// <summary>Issues Solved this Week — the Week's decisions.</summary>
        public IReadOnlyList<Issue> Decisions { get; init; } = new List<Issue>();

        /// <summary>To-Dos created in-meeting this Week — the Week's actions.</summary>
        public IReadOnlyList<ToDo> Actions { get; init; } = new List<ToDo>();

        /// <summary>This Week's Risk-type Headlines — the news-round risks.</summary>
        public IReadOnlyList<Headline> RiskHeadlines { get; init; } = new List<Headline>();

        /// <summary>High/Critical Issues still Open as of this Week — the carried risks.</summary>
        public IReadOnlyList<Issue> RiskIssues { get; init; } = new List<Issue>();

        /// <summary>The single suggested-format summary sentence over the Week's tallies.</summary>
        public string SummaryText { get; init; } = "";
    }
}
