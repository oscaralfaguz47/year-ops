using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Models.ViewModels.WeeklyPulse
{
    /// <summary>
    /// Backs the Weekly Pulse Dashboard for a single Week: each team in meeting order
    /// alongside its check-in for <see cref="WeekStart"/> (null when the Week is blank)
    /// and its living Issues shown as of <see cref="WeekStart"/>.
    /// </summary>
    public class DashboardVM
    {
        public DateOnly WeekStart { get; set; }
        public List<TeamCheckInVM> Teams { get; set; } = new();

        /// <summary>Candidate owners (id + display name) for the To-Do owner dropdown.</summary>
        public List<PersonOptionVM> People { get; set; } = new();
    }

    public class TeamCheckInVM
    {
        public Team Team { get; set; }

        /// <summary>The team's check-in for the Week, or null when not yet recorded.</summary>
        public CheckIn CheckIn { get; set; }

        /// <summary>The team's living Issues with their state as of the Week.</summary>
        public List<IssueRowVM> Issues { get; set; } = new();

        /// <summary>The team's living To-Dos (non-Done) with their state as of the Week.</summary>
        public List<ToDoRowVM> ToDos { get; set; } = new();

        /// <summary>
        /// The team's readiness signal for the Week, derived only from its KPIs
        /// (see <see cref="ReadinessService"/>).
        /// </summary>
        public ReadinessState Readiness { get; set; }

        /// <summary>The team's KPIs that expect input this Week, each with its result (or null).</summary>
        public List<KpiResultRowVM> Kpis { get; set; } = new();

        /// <summary>The team's headlines posted this Week (snapshot — blank each new Week).</summary>
        public List<Headline> Headlines { get; set; } = new();
    }

    public class KpiResultRowVM
    {
        public KpiDefinition Kpi { get; set; }

        /// <summary>The KPI's result for the Week, or null when not yet recorded.</summary>
        public KpiResult Result { get; set; }
    }

    public class IssueRowVM
    {
        public Issue Issue { get; set; }

        /// <summary>The Issue's state as of the Dashboard's Week (derived, not stored).</summary>
        public IssueStatus State { get; set; }
    }

    public class ToDoRowVM
    {
        public ToDo ToDo { get; set; }

        /// <summary>The To-Do's state as of the Week (derived, not stored).</summary>
        public ToDoStatus State { get; set; }

        /// <summary>
        /// How the To-Do surfaces in the Review (Blocked loud, Open quiet, Done hidden).
        /// Set only on the Review surface; the Dashboard reads <see cref="State"/> directly.
        /// </summary>
        public ToDoSurfacing Surfacing { get; set; }
    }
}
