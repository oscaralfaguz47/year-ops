using OceansApp.Models.Models;

namespace OceansApp.Models.ViewModels.WeeklyPulse
{
    /// <summary>
    /// Backs the Weekly Pulse Dashboard for a single Week: each team in meeting order
    /// alongside its check-in for <see cref="WeekStart"/> (null when the Week is blank).
    /// </summary>
    public class DashboardVM
    {
        public DateOnly WeekStart { get; set; }
        public List<TeamCheckInVM> Teams { get; set; } = new();
    }

    public class TeamCheckInVM
    {
        public Team Team { get; set; }

        /// <summary>The team's check-in for the Week, or null when not yet recorded.</summary>
        public CheckIn CheckIn { get; set; }
    }
}
