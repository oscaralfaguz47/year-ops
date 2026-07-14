namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// The per-Team, per-Week readiness signal, computed purely from Active KPIs
    /// (see <see cref="ReadinessService"/> and CONTEXT.md). Check-ins, headlines, and issues
    /// never affect it, and it is independent of Review-inclusion.
    /// </summary>
    public enum ReadinessState
    {
        /// <summary>The Team has no Active KPI definitions — a setup gap.</summary>
        NotConfigured = 0,

        /// <summary>Some Active KPI still lacks a result for the current Week.</summary>
        NotReady = 1,

        /// <summary>Every Active KPI has a result this Week (Red counts — reported, not healthy).</summary>
        Ready = 2
    }
}
