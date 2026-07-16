using OceansApp.Models.Models;

namespace OceansApp.Models.Domain.WeeklyPulse
{
    /// <summary>
    /// Pure domain logic for a Team's <see cref="ReadinessState"/> in a given Week, derived
    /// <b>only</b> from its KPIs. A KPI counts toward readiness iff it is Active
    /// (<see cref="KpiScopeService.ExpectsInput"/>); retired KPIs are ignored, and check-ins,
    /// headlines, and issues never affect it. Readiness is <b>decoupled from review-inclusion</b>:
    /// a result counts as reported whether or not it is included in the Review.
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
            var active = kpis.Where(KpiScopeService.ExpectsInput).ToList();

            // No Active KPI definitions — a setup gap, never counts as ready.
            if (active.Count == 0)
            {
                return ReadinessState.NotConfigured;
            }

            var reported = resultsThisWeek.Select(r => r.KpiDefinitionId).ToHashSet();

            // Ready iff every Active KPI has a result this Week (a result always carries a
            // selected status); otherwise some Active KPI is still missing its result.
            return active.All(k => reported.Contains(k.KpiDefinitionId))
                ? ReadinessState.Ready
                : ReadinessState.NotReady;
        }
    }
}
