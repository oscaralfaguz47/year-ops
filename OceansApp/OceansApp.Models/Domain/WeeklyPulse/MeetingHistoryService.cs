using OceansApp.Models.Models;

namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// Pure domain logic for the Weekly Pulse Meeting History (minutes). Past Weeks are
    /// read back as minutes: each Week shows its snapshots plus every living entity that
    /// was <b>active that Week</b>, shown <i>as of</i> that Week — so a single Issue/To-Do
    /// appears under each Week it touched, with its per-week progression.
    ///
    /// <para><b>activeInWeek</b> is a pure derivation, not stored: an entity is active in a
    /// Week iff it <i>surfaced in that Week's Review</i> (its as-of state surfaces) OR it had
    /// a status/comment row that Week (it was commented on or status-changed). It composes
    /// the existing <see cref="IssueStateService"/>/<see cref="ToDoStateService"/> selectors
    /// and the <see cref="ReviewSurfacingService"/> surfacing rules.</para>
    ///
    /// Has <b>no EF Core / HttpContext dependency</b>: it operates purely on plain history
    /// rows and week stamps, so it is fully unit-testable. See ADR 0001 and CONTEXT.md.
    /// </summary>
    public static class MeetingHistoryService
    {
        /// <summary>
        /// The set of past Weeks = the distinct <see cref="DateOnly"/> WeekStart values
        /// across the data (there is no Week table — see ADR 0001), newest Week first.
        /// The caller gathers the stamps from every week-stamped source (check-ins, issue
        /// and to-do history, KPI results, headlines).
        /// </summary>
        public static IReadOnlyList<DateOnly> DistinctWeeks(IEnumerable<DateOnly> weekStarts) =>
            weekStarts.Distinct().OrderByDescending(w => w).ToList();

        /// <summary>
        /// Whether an <see cref="Issue"/> was active in <paramref name="week"/>: it surfaced
        /// in that Week's Review (its as-of state surfaces, per
        /// <see cref="ReviewSurfacingService.Surfaces"/>) OR it had a status/comment row that
        /// Week. Pure: derived from the history rows.
        ///
        /// <para>An entity can never be active <i>before it existed</i>: a Week earlier than
        /// <paramref name="originWeekStart"/> is not active, regardless of what
        /// <see cref="IssueStateService.StateAsOf"/> would default to (it returns Open when no
        /// row is on/before the Week, which would otherwise surface the Issue in pre-origin
        /// Weeks). The live pin is deliberately <b>not</b> consulted: a pin is a mutable
        /// current-meeting flag, not week-stamped, so feeding it into historical surfacing
        /// would let toggling a pin today rewrite past minutes.</para>
        /// </summary>
        public static bool IssueActiveInWeek(IEnumerable<IssueHistory> history, DateOnly originWeekStart, DateOnly week)
        {
            if (week < originWeekStart) return false;
            var rows = history as ICollection<IssueHistory> ?? history.ToList();
            var touchedThisWeek = rows.Any(h => h.WeekStart == week);
            var surfaces = ReviewSurfacingService.Surfaces(
                IssueStateService.StateAsOf(rows, week), pinned: false);
            return touchedThisWeek || surfaces;
        }

        /// <summary>
        /// Whether a <see cref="ToDo"/> was active in <paramref name="week"/>: it surfaced in
        /// that Week's Review (its as-of state is not <see cref="ToDoSurfacing.Hidden"/>, per
        /// <see cref="ReviewSurfacingService.SurfaceToDo"/>) OR it had a status/comment row
        /// that Week. Pure: derived from the history rows. Mirrors
        /// <see cref="IssueActiveInWeek"/> (a To-Do has no pin), including the pre-origin
        /// guard: a Week earlier than <paramref name="originWeekStart"/> is never active even
        /// though <see cref="ToDoStateService.StateAsOf"/> defaults to Open there.
        /// </summary>
        public static bool ToDoActiveInWeek(IEnumerable<ToDoHistory> history, DateOnly originWeekStart, DateOnly week)
        {
            if (week < originWeekStart) return false;
            var rows = history as ICollection<ToDoHistory> ?? history.ToList();
            var touchedThisWeek = rows.Any(h => h.WeekStart == week);
            var surfaces = ReviewSurfacingService.SurfaceToDo(
                ToDoStateService.StateAsOf(rows, week)) != ToDoSurfacing.Hidden;
            return touchedThisWeek || surfaces;
        }
    }
}
