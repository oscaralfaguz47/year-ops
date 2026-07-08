using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Models.ViewModels.WeeklyPulse
{
    /// <summary>
    /// Backs the Weekly Pulse Review (meeting) view for a single Week: each Team in
    /// meeting order with only its <b>surfaced</b> Issues — the union of Open issues
    /// and pinned-Deferred issues, never Solved (see
    /// <c>ReviewSurfacingService.Surfaces</c>). Un-pinned Deferred and Solved issues
    /// are deliberately hidden; <see cref="HiddenHint"/> explains that to the room.
    /// </summary>
    public class ReviewVM
    {
        public DateOnly WeekStart { get; set; }
        public List<ReviewTeamVM> Teams { get; set; } = new();

        /// <summary>
        /// Candidate owners (id + display name) for the live-edit moments (WP-CR4): the To-Do
        /// edit form's owner picker and the issue -> To-Do conversion's owner picker.
        /// </summary>
        public List<PersonOptionVM> People { get; set; } = new();

        /// <summary>A short note explaining what Review deliberately hides.</summary>
        public string HiddenHint { get; set; } =
            "Hidden on purpose: Solved issues are done, and Deferred issues stay parked " +
            "unless pinned. Pin a Deferred issue on the Dashboard to surface it here.";
    }

    public class ReviewTeamVM
    {
        public Team Team { get; set; }

        /// <summary>
        /// The Team's headlines for the Week — the news round that opens its segment.
        /// Every headline surfaces; <see cref="HeadlineRowVM.Emphasis"/> says how loud.
        /// </summary>
        public List<HeadlineRowVM> Headlines { get; set; } = new();

        /// <summary>
        /// The Team's surfaced KPIs for the Week — in-scope only (out-of-scope/retired KPIs
        /// are dropped), Loud (Red/Yellow/missing) before Quiet (Green). See
        /// <see cref="ReviewSurfacingService.SurfaceKpi"/>.
        /// </summary>
        public List<KpiReviewRowVM> Kpis { get; set; } = new();

        /// <summary>The Team's surfaced Issues for the Week, in priority order.</summary>
        public List<IssueRowVM> Issues { get; set; } = new();

        /// <summary>
        /// The Team's surfaced To-Dos for the Week — every non-Done To-Do, Blocked flagged
        /// loud (see <see cref="ToDoRowVM.Surfacing"/>). Done to-dos are dropped.
        /// </summary>
        public List<ToDoRowVM> ToDos { get; set; } = new();
    }

    public class HeadlineRowVM
    {
        public Headline Headline { get; set; }

        /// <summary>Loud (Risk) or Quiet (Highlight) — every headline surfaces.</summary>
        public HeadlineEmphasis Emphasis { get; set; }
    }

    public class KpiReviewRowVM
    {
        public KpiDefinition Kpi { get; set; }

        /// <summary>The KPI's result for the Week, or null when not yet recorded.</summary>
        public KpiResult Result { get; set; }

        /// <summary>How the KPI surfaces in Review — Loud (Red/Yellow/missing) or Quiet (Green).</summary>
        public KpiSurfacing Surfacing { get; set; }
    }
}
