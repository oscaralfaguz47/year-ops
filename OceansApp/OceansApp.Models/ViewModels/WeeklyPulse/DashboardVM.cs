using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Models.ViewModels.WeeklyPulse
{
    /// <summary>
    /// Backs the Weekly Pulse Dashboard for a single Week: each team in meeting order
    /// with its living Issues shown as of <see cref="WeekStart"/> and its per-Week
    /// snapshots (KPI results, headlines).
    /// </summary>
    public class DashboardVM
    {
        public DateOnly WeekStart { get; set; }
        public List<TeamCheckInVM> Teams { get; set; } = new();

        /// <summary>Candidate owners (id + display name) for the To-Do owner dropdown.</summary>
        public List<PersonOptionVM> People { get; set; } = new();

        /// <summary>
        /// The TeamIds currently in view, driven by the <c>?teams=</c> querystring so a
        /// filtered view is bookmarkable. Defaults to every team (no filter). The chip bar
        /// iterates <see cref="Teams"/> for the full list; only ids in this set render.
        /// </summary>
        public HashSet<int> SelectedTeamIds { get; set; } = new();
    }

    public class TeamCheckInVM
    {
        public Team Team { get; set; }

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

    /// <summary>A Team option (id + name) for the edit-form Team pickers.</summary>
    public class TeamOptionVM
    {
        public int TeamId { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Backs the shared Issue edit form partial (<c>_IssueEditForm</c>). Self-contained so both
    /// the Dashboard and the Review (WP-CR4 live-edit moments) can render the same form: it
    /// carries the Issue's current editable values plus the Team option list. Status is edited
    /// alongside the Issue's own fields (Team/title/priority) and applied through history.
    /// </summary>
    public class IssueEditFormVM
    {
        public int IssueId { get; set; }
        public int TeamId { get; set; }
        public string Title { get; set; }
        public IssuePriority Priority { get; set; }

        /// <summary>The Issue's current state as of the Week (pre-selects the status dropdown).</summary>
        public IssueStatus State { get; set; }

        public List<TeamOptionVM> Teams { get; set; } = new();
    }

    /// <summary>
    /// Backs the shared To-Do edit form partial (<c>_ToDoEditForm</c>). Self-contained so both
    /// the Dashboard and the Review can render the same form: it carries the To-Do's current
    /// editable values (Team/owner/title/due date) plus the Team and Person option lists. The
    /// status lifecycle is edited separately and is not part of this form.
    /// </summary>
    public class ToDoEditFormVM
    {
        public int ToDoId { get; set; }
        public int TeamId { get; set; }
        public string Title { get; set; }
        public string OwnerId { get; set; }
        public DateOnly DueDate { get; set; }

        public List<TeamOptionVM> Teams { get; set; } = new();
        public List<PersonOptionVM> People { get; set; } = new();
    }
}
