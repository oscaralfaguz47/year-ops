using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Pure-logic tests for <see cref="ReadinessService"/>: the per-Team/Week three-state signal
    /// is derived from Active KPIs only (see CONTEXT.md Readiness). A KPI counts iff it is Active
    /// (<see cref="KpiScopeService.ExpectsInput"/>); readiness means <i>reported</i>, not
    /// <i>healthy</i>, so a Red result still counts, and it is independent of Review-inclusion.
    /// </summary>
    public class ReadinessTests
    {
        private static readonly DateOnly Week = new(2026, 6, 29);

        private static KpiDefinition Kpi(int id, bool active = true) =>
            new()
            {
                KpiDefinitionId = id,
                Name = $"KPI {id}",
                Target = ">= 95%",
                Active = active
            };

        private static KpiResult Result(int kpiDefinitionId, KpiStatus status = KpiStatus.Green, bool includeInReview = true) =>
            new()
            {
                KpiDefinitionId = kpiDefinitionId,
                WeekStart = Week,
                Value = "92%",
                Status = status,
                IncludeInReview = includeInReview
            };

        [Fact]
        public void NotConfigured_WhenNoActiveKpis()
        {
            // No KPIs at all.
            Assert.Equal(ReadinessState.NotConfigured,
                ReadinessService.Evaluate(Array.Empty<KpiDefinition>(), Array.Empty<KpiResult>()));

            // KPIs exist but none are Active (all retired) — a setup gap for the Week.
            var kpis = new[] { Kpi(1, active: false), Kpi(2, active: false) };
            Assert.Equal(ReadinessState.NotConfigured,
                ReadinessService.Evaluate(kpis, Array.Empty<KpiResult>()));
        }

        [Fact]
        public void NotReady_WhenSomeActiveKpiLacksAResultThisWeek()
        {
            var kpis = new[] { Kpi(1), Kpi(2) };
            // Only KPI 1 has a result this Week; KPI 2 is missing.
            var results = new[] { Result(1) };

            Assert.Equal(ReadinessState.NotReady, ReadinessService.Evaluate(kpis, results));
        }

        [Fact]
        public void Ready_WhenEveryActiveKpiHasAResult_RedCounts()
        {
            var kpis = new[] { Kpi(1), Kpi(2) };
            // Every Active KPI reported; a Red result still counts as reported.
            var results = new[] { Result(1, KpiStatus.Green), Result(2, KpiStatus.Red) };

            Assert.Equal(ReadinessState.Ready, ReadinessService.Evaluate(kpis, results));
        }

        [Fact]
        public void RetiredKpis_DoNotAffectReadiness()
        {
            // One Active KPI (reported) plus a retired KPI with no result.
            var kpis = new[] { Kpi(1, active: true), Kpi(2, active: false) };
            var results = new[] { Result(1) };

            // Only the Active KPI matters, and it is reported, so the Team is Ready.
            Assert.Equal(ReadinessState.Ready, ReadinessService.Evaluate(kpis, results));
        }

        [Fact]
        public void Readiness_IsDecoupledFromReviewInclusion()
        {
            var kpis = new[] { Kpi(1), Kpi(2) };
            // Both Active KPIs reported, but neither result is included in the Review — still Ready,
            // because Readiness counts a result as reported regardless of inclusion.
            var results = new[]
            {
                Result(1, includeInReview: false),
                Result(2, includeInReview: false)
            };

            Assert.Equal(ReadinessState.Ready, ReadinessService.Evaluate(kpis, results));
        }
    }
}
