namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// One moment in the guided sequential Weekly Pulse Review (WP-CR4). The Review is a
    /// script, not a board: each Team is walked through these moments in this fixed order
    /// (the numeric values encode that order). Each moment has its own edit rule — see
    /// <see cref="ReviewScriptService"/>.
    /// </summary>
    public enum ReviewMoment
    {
        /// <summary>Unsaved sharing moment — a static header, nothing is stored.</summary>
        CheckIn = 0,

        /// <summary>KPI review — view-only.</summary>
        Kpi = 1,

        /// <summary>Headlines — view-only; the only action is dropping a headline to an Issue.</summary>
        Headlines = 2,

        /// <summary>Issues — fully editable live; an Issue can spawn a To-Do.</summary>
        Issues = 3,

        /// <summary>To-Dos — fully editable live.</summary>
        ToDos = 4
    }
}
