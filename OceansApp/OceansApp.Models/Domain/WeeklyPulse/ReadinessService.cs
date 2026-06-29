using OceansApp.Models.Models;

namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// Pure domain logic for a Team's <see cref="ReadinessState"/> in a given Week, derived
    /// <b>only</b> from its KPIs. A KPI is in scope for readiness iff it is in the meeting scope
    /// (<see cref="KpiScopeService.SurfacesInReview"/> — i.e. both Active and InScope); retired
    /// or out-of-scope KPIs are ignored, and check-ins, headlines, and issues never affect it.
    ///
    /// Readiness means <i>reported</i>, not <i>healthy</i>: a Red result still counts. Has
    /// <b>no EF Core / HttpContext dependency</b>, mirroring <see cref="KpiScopeService"/>, so it
    /// is fully unit-testable. See CONTEXT.md (Readiness).
    /// </summary>
    public static class ReadinessService
    {
        /// <summary>
        /// Computes the readiness signal from a Team's KPI definitions and the KPI results
        /// recorded for the Week in question. Only the (KPI, Week) results need be passed —
        /// any extra results are matched by <see cref="KpiResult.KpiDefinitionId"/>.
        /// </summary>
        public static ReadinessState Evaluate(
            IEnumerable<KpiDefinition> kpis,
            IEnumerable<KpiResult> resultsThisWeek)
        {
            var inScope = kpis.Where(KpiScopeService.SurfacesInReview).ToList();

            // No in-meeting-scope KPI definitions — a setup gap, never counts as ready.
            if (inScope.Count == 0)
            {
                return ReadinessState.NotConfigured;
            }

            var reported = resultsThisWeek.Select(r => r.KpiDefinitionId).ToHashSet();

            // Ready iff every in-scope KPI has a result this Week (a result always carries a
            // selected status); otherwise some in-scope KPI is still missing its result.
            return inScope.All(k => reported.Contains(k.KpiDefinitionId))
                ? ReadinessState.Ready
                : ReadinessState.NotReady;
        }
    }
}
