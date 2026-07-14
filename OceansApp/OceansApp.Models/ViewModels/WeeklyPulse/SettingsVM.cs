using OceansApp.Models.Models;

namespace OceansApp.Models.ViewModels.WeeklyPulse
{
    /// <summary>
    /// Backs the Weekly Pulse Settings page (Administer-gated): the Teams in meeting order and,
    /// per Team, its KPI definitions. People available to be Team Leaders / KPI Owners are listed
    /// once for the dropdowns. A Person may lead more than one Team, so the same person can appear
    /// as the leader of several rows.
    /// </summary>
    public class SettingsVM
    {
        public List<TeamSettingsVM> Teams { get; set; } = new();

        /// <summary>Candidate leaders / owners (id + display name) for the dropdowns.</summary>
        public List<PersonOptionVM> People { get; set; } = new();
    }

    public class TeamSettingsVM
    {
        public Team Team { get; set; }
        public List<KpiDefinition> Kpis { get; set; } = new();
    }

    public class PersonOptionVM
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
    }
}
