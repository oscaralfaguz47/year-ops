using OceansApp.Models.Models;

namespace OceansApp.Areas.WeeklyPulse.ViewHelpers
{
    /// <summary>
    /// Single source of truth for the Weekly Pulse KPI status → Bootstrap badge CSS map,
    /// shared by every view that renders a <see cref="KpiStatus"/> badge (the Dashboard,
    /// Review and KPI/Meeting History views) so the colour scheme is defined once.
    /// </summary>
    public static class KpiBadge
    {
        public static string Css(KpiStatus status) => status switch
        {
            KpiStatus.Green => "bg-success",
            KpiStatus.Yellow => "bg-warning text-dark",
            _ => "bg-danger"
        };
    }
}
