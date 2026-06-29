namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// The per-Team, per-Week readiness signal, computed purely from in-meeting-scope KPIs
    /// (see <see cref="ReadinessService"/> and CONTEXT.md). Check-ins, headlines, and issues
    /// never affect it.
    /// </summary>
    public enum ReadinessState
    {
        /// <summary>The Team has no in-meeting-scope KPI definitions — a setup gap.</summary>
        NotConfigured = 0,

        /// <summary>Some in-scope KPI still lacks a result for the current Week.</summary>
        NotReady = 1,

        /// <summary>Every in-scope KPI has a result this Week (Red counts — reported, not healthy).</summary>
        Ready = 2
    }
}
