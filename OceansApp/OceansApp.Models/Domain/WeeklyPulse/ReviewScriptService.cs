namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// Pure domain logic for the guided sequential Weekly Pulse Review (WP-CR4). The Review
    /// is walked as a script, not a board: each Team runs through a fixed ordered sequence of
    /// <see cref="ReviewMoment"/>s, each with its own edit rule. This service is the single
    /// source of truth the Review view walks, so the moment order on screen and the edit
    /// affordances each moment offers are asserted here rather than buried in markup.
    ///
    /// Has <b>no EF Core / HttpContext dependency</b>, so it is fully unit-testable. Mirrors
    /// <see cref="ReviewSurfacingService"/>; see ADR 0001 and CONTEXT.md.
    /// </summary>
    public static class ReviewScriptService
    {
        /// <summary>
        /// The guided walk, in fixed order: Check-in -> KPI -> Headlines -> Issues -> To-Dos.
        /// (Ordered by each moment's numeric value.)
        /// </summary>
        public static IReadOnlyList<ReviewMoment> Moments { get; } = new[]
        {
            ReviewMoment.CheckIn,
            ReviewMoment.Kpi,
            ReviewMoment.Headlines,
            ReviewMoment.Issues,
            ReviewMoment.ToDos
        };

        /// <summary>
        /// Whether a moment's records are editable live in place. Only Issues and To-Dos are
        /// fully editable (reusing the WP-CR2 forms); Check-in, KPI and Headlines are read-only.
        /// </summary>
        public static bool IsEditable(ReviewMoment moment) =>
            moment == ReviewMoment.Issues || moment == ReviewMoment.ToDos;

        /// <summary>
        /// Whether a moment offers "drop to Issue". Only the Headlines moment does — a headline
        /// is view-only apart from being converted into an Issue.
        /// </summary>
        public static bool CanDropToIssue(ReviewMoment moment) =>
            moment == ReviewMoment.Headlines;

        /// <summary>
        /// Whether a moment offers spawning a To-Do. Only the Issues moment does (issue -> To-Do);
        /// a headline never spawns a To-Do directly — it can only become an Issue first.
        /// </summary>
        public static bool CanSpawnToDo(ReviewMoment moment) =>
            moment == ReviewMoment.Issues;
    }
}
