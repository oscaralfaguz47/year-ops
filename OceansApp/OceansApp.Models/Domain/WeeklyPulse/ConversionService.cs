using OceansApp.Models.Models;

namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// Pure domain rules for Weekly Pulse <b>Conversions</b> (WP-2.6): turning a source
    /// record into a new Living entity. Conversions are always <b>additive</b> — the
    /// source is never consumed or deleted (the repository leaves it intact in its Week);
    /// these rules only build the new, pre-filled entity and stamp it with an origin
    /// back-reference (<see cref="OriginType"/> + origin id).
    ///
    /// Has <b>no EF Core / HttpContext dependency</b>: it operates on plain model objects,
    /// so the mapping is fully unit-testable. The new entity's
    /// <c>OriginWeekStart</c> is the Week the conversion happens in, not the source's Week.
    /// See the Conversion entry in docs/oce-weekly-pulse/CONTEXT.md.
    /// </summary>
    public static class ConversionService
    {
        /// <summary>
        /// Pre-fills a new <see cref="Issue"/> from a <see cref="CheckIn"/> (segue),
        /// back-referencing the source check-in. Priority defaults to
        /// <see cref="IssuePriority.Med"/>.
        /// </summary>
        public static Issue FromCheckIn(CheckIn source, DateOnly weekStart) => new()
        {
            TeamId = source.TeamId,
            Title = $"[from check-in] {source.Note}",
            Priority = IssuePriority.Med,
            OriginWeekStart = weekStart,
            OriginType = OriginType.CheckIn,
            OriginId = source.CheckInId
        };

        /// <summary>
        /// Pre-fills a new <see cref="Issue"/> from a <see cref="Headline"/>,
        /// back-referencing the source headline. A <see cref="HeadlineType.Risk"/> maps to
        /// <see cref="IssuePriority.High"/>; a Highlight maps to <see cref="IssuePriority.Med"/>.
        /// </summary>
        public static Issue FromHeadline(Headline source, DateOnly weekStart) => new()
        {
            TeamId = source.TeamId,
            Title = $"[from headline] {source.Text}",
            Priority = source.Type == HeadlineType.Risk ? IssuePriority.High : IssuePriority.Med,
            OriginWeekStart = weekStart,
            OriginType = OriginType.Headline,
            OriginId = source.HeadlineId
        };

        /// <summary>
        /// Pre-fills a new <see cref="ToDo"/> from an <see cref="Issue"/>, back-referencing
        /// the source issue. The To-Do's required <paramref name="ownerId"/> and
        /// <paramref name="dueDate"/> are supplied by the caller (an Issue carries neither).
        /// </summary>
        public static ToDo FromIssue(Issue source, string ownerId, DateOnly dueDate, DateOnly weekStart) => new()
        {
            TeamId = source.TeamId,
            Title = $"[from issue] {source.Title}",
            OwnerId = ownerId,
            DueDate = dueDate,
            OriginWeekStart = weekStart,
            OriginType = OriginType.Issue,
            OriginId = source.IssueId
        };
    }
}
