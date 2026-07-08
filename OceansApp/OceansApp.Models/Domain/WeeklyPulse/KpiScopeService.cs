using OceansApp.Models.Models;

namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// Pure domain logic for a KPI's two orthogonal questions, now split across the two records:
    /// whether the <see cref="KpiDefinition"/> expects input (structural, drives Readiness) and
    /// whether a given Week's <see cref="KpiResult"/> surfaces in the Review (a per-Week decision).
    ///
    /// Retiring a KPI (setting <see cref="KpiDefinition.Active"/> to false) keeps its historical
    /// results but stops it expecting new input — it drops from Readiness. Review-inclusion is no
    /// longer a definition flag: it is decided per Week on the result via
    /// <see cref="KpiResult.IncludeInReview"/>, and a Week with no result never surfaces.
    ///
    /// Has <b>no EF Core / HttpContext dependency</b>, mirroring <see cref="ReviewSurfacingService"/>,
    /// so it is fully unit-testable. See ADR 0002 and CONTEXT.md.
    /// </summary>
    public static class KpiScopeService
    {
        /// <summary>
        /// Whether the KPI still expects new input this Week (i.e. counts toward Readiness):
        /// iff it is <see cref="KpiDefinition.Active"/>. Retired KPIs never expect input.
        /// </summary>
        public static bool ExpectsInput(KpiDefinition kpi) => kpi.Active;

        /// <summary>
        /// Whether this Week's result surfaces in the meeting (Review): iff a result exists for the
        /// Week (<paramref name="result"/> is non-null) and its
        /// <see cref="KpiResult.IncludeInReview"/> is set. A KPI with no result this Week, or one
        /// whose result was un-ticked, does not appear — independent of Readiness.
        /// </summary>
        public static bool SurfacesInReview(KpiResult? result) => result is { IncludeInReview: true };
    }
}
