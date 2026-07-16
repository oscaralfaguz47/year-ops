using OceansApp.Models.Models;

namespace OceansApp.Models.ViewModels.WeeklyPulse
{
    /// <summary>
    /// Backs the Weekly Pulse Meeting History index: the list of past Weeks read as
    /// minutes. The set of Weeks is the distinct WeekStart values across the data (there is
    /// no Week table — see ADR 0001), newest first
    /// (see <c>MeetingHistoryService.DistinctWeeks</c>).
    /// </summary>
    public class MeetingHistoryVM
    {
        public List<DateOnly> Weeks { get; set; } = new();
    }

    /// <summary>
    /// Backs the minutes for a single past Week: each Team with that Week's snapshots
    /// (headlines, KPI results) plus every living Issue/To-Do that was
    /// <b>active that Week</b> (see <c>MeetingHistoryService.IssueActiveInWeek</c> /
    /// <c>ToDoActiveInWeek</c>), shown <i>as of</i> that Week — so a single entity appears
    /// under each Week it touched, with its per-week progression.
    /// </summary>
    public class WeekMinutesVM
    {
        public DateOnly WeekStart { get; set; }
        public List<WeekMinutesTeamVM> Teams { get; set; } = new();
    }

    public class WeekMinutesTeamVM
    {
        public Team Team { get; set; }

        /// <summary>The Team's headline snapshots posted that Week.</summary>
        public List<Headline> Headlines { get; set; } = new();

        /// <summary>The Team's KPI results recorded that Week (snapshot), each with its definition.</summary>
        public List<KpiResultRowVM> Kpis { get; set; } = new();

        /// <summary>The Team's Issues active that Week, each shown with its as-of state.</summary>
        public List<IssueRowVM> Issues { get; set; } = new();

        /// <summary>The Team's To-Dos active that Week, each shown with its as-of state.</summary>
        public List<ToDoRowVM> ToDos { get; set; } = new();
    }
}
