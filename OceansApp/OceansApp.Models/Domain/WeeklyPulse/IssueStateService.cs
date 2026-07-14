using OceansApp.Models.Models;

namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// Pure domain logic for a Weekly Pulse <see cref="Issue"/>'s living state.
    ///
    /// Has <b>no EF Core / HttpContext dependency</b>: it operates purely on plain
    /// <see cref="IssueHistory"/> rows, so it is fully unit-testable and is the bit
    /// worth lifting across surfaces. See ADR 0001 and CONTEXT.md.
    /// </summary>
    public static class IssueStateService
    {
        /// <summary>
        /// The Issue's state as of <paramref name="week"/> = the latest status row with
        /// <see cref="IssueHistory.WeekStart"/> on or before <paramref name="week"/>.
        /// Comment rows are ignored. Within a Week, rows are ordered by
        /// <see cref="IssueHistory.CreatedAt"/> then insertion order
        /// (<see cref="IssueHistory.IssueHistoryId"/>). Defaults to
        /// <see cref="IssueStatus.Open"/> when no status row applies.
        /// </summary>
        public static IssueStatus StateAsOf(IEnumerable<IssueHistory> history, DateOnly week)
        {
            var latest = history
                .Where(h => h.ChangeType == IssueChangeType.Status && h.WeekStart <= week)
                .OrderBy(h => h.WeekStart)
                .ThenBy(h => h.CreatedAt)
                .ThenBy(h => h.IssueHistoryId)
                .LastOrDefault();

            return latest?.Status ?? IssueStatus.Open;
        }
    }
}
