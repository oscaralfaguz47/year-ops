using OceansApp.Models.Models;

namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// Pure domain logic for how a <see cref="KpiDefinition"/>'s two independent flags
    /// (<see cref="KpiDefinition.Active"/> and <see cref="KpiDefinition.InScope"/>) combine.
    ///
    /// Retiring a KPI (setting <see cref="KpiDefinition.Active"/> to false) keeps its historical
    /// results but stops it expecting new input — it drops from Readiness and from the Review.
    /// Whether it appears in the meeting additionally requires it to be in scope.
    ///
    /// Has <b>no EF Core / HttpContext dependency</b>, mirroring <see cref="ReviewSurfacingService"/>,
    /// so it is fully unit-testable. See ADR 0002 and CONTEXT.md.
    /// </summary>
    public static class KpiScopeService
    {
        /// <summary>
        /// Whether the KPI still expects new input this Week (i.e. counts toward Readiness):
        /// iff it is <see cref="KpiDefinition.Active"/>. Retired KPIs never expect input,
        /// regardless of scope.
        /// </summary>
        public static bool ExpectsInput(KpiDefinition kpi) => kpi.Active;

        /// <summary>
        /// Whether the KPI surfaces in the meeting (Review): iff it is both
        /// <see cref="KpiDefinition.Active"/> and <see cref="KpiDefinition.InScope"/>.
        /// Retiring it, or moving it out of scope, drops it from the Review.
        /// </summary>
        public static bool SurfacesInReview(KpiDefinition kpi) => kpi.Active && kpi.InScope;
    }
}
