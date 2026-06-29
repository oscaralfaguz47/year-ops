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

        /// <summary>The Team's surfaced Issues for the Week, in priority order.</summary>
        public List<IssueRowVM> Issues { get; set; } = new();
    }

    public class HeadlineRowVM
    {
        public Headline Headline { get; set; }

        /// <summary>Loud (Risk) or Quiet (Highlight) — every headline surfaces.</summary>
        public HeadlineEmphasis Emphasis { get; set; }
    }
}
