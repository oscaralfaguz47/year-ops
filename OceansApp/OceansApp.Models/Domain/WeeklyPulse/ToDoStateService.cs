using OceansApp.Models.Models;

namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// Pure domain logic for a Weekly Pulse <see cref="ToDo"/>'s living state. Mirrors
    /// <see cref="IssueStateService"/>.
    ///
    /// Has <b>no EF Core / HttpContext dependency</b>: it operates purely on plain
    /// <see cref="ToDoHistory"/> rows, so it is fully unit-testable and is the bit worth
    /// lifting across surfaces. See ADR 0001 and CONTEXT.md.
    /// </summary>
    public static class ToDoStateService
    {
        /// <summary>
        /// The To-Do's state as of <paramref name="week"/> = the latest status row with
        /// <see cref="ToDoHistory.WeekStart"/> on or before <paramref name="week"/>.
        /// Comment rows are ignored. Within a Week, rows are ordered by
        /// <see cref="ToDoHistory.CreatedAt"/> then insertion order
        /// (<see cref="ToDoHistory.ToDoHistoryId"/>). Defaults to
        /// <see cref="ToDoStatus.Open"/> when no status row applies.
        /// </summary>
        public static ToDoStatus StateAsOf(IEnumerable<ToDoHistory> history, DateOnly week)
        {
            var latest = history
                .Where(h => h.ChangeType == ToDoChangeType.Status && h.WeekStart <= week)
                .OrderBy(h => h.WeekStart)
                .ThenBy(h => h.CreatedAt)
                .ThenBy(h => h.ToDoHistoryId)
                .LastOrDefault();

            return latest?.Status ?? ToDoStatus.Open;
        }
    }
}
