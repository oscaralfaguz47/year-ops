using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Models.ViewModels.WeeklyPulse
{
    /// <summary>
    /// Backs the Weekly Pulse <b>Weekly Summary</b> for a single Week: each Team in meeting
    /// order with its derived, READ-ONLY draft (see <c>WeeklySummaryService.Derive</c>).
    /// Nothing here is stored — the Summary is re-computed on the fly from the Week's data
    /// each time it is read. Editing/override is out of scope for v1 (see ADR 0001).
    /// </summary>
    public class WeeklySummaryVM
    {
        public DateOnly WeekStart { get; set; }
        public List<WeeklySummaryTeamVM> Teams { get; set; } = new();

        /// <summary>A short note that the draft is derived and read-only.</summary>
        public string DerivedHint { get; set; } =
            "This summary is assembled automatically from the Week's data — decisions are the " +
            "issues solved, actions the to-dos raised, risks the risk headlines and open " +
            "High/Critical issues. It is read-only: nothing here is saved.";
    }

    public class WeeklySummaryTeamVM
    {
        public Team Team { get; set; }

        /// <summary>The Team's derived Weekly Summary draft for the Week.</summary>
        public WeeklySummary Summary { get; set; }
    }
}
