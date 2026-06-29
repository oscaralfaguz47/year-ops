using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Pure-logic tests for <see cref="ReadinessService"/>: the per-Team/Week three-state
    /// signal is derived from in-meeting-scope KPIs only (see CONTEXT.md Readiness). A KPI
    /// is in scope iff it is both Active and InScope (<see cref="KpiScopeService.SurfacesInReview"/>);
    /// readiness means <i>reported</i>, not <i>healthy</i>, so a Red result still counts.
    /// </summary>
    public class ReadinessTests
    {
        private static readonly DateOnly Week = new(2026, 6, 29);

        private static KpiDefinition Kpi(int id, bool active = true, bool inScope = true) =>
            new()
            {
                KpiDefinitionId = id,
                Name = $"KPI {id}",
                Target = ">= 95%",
                Active = active,
                InScope = inScope
            };

        private static KpiResult Result(int kpiDefinitionId, KpiStatus status = KpiStatus.Green) =>
            new()
            {
                KpiDefinitionId = kpiDefinitionId,
                WeekStart = Week,
                Value = "92%",
                Status = status
            };

        [Fact]
        public void NotConfigured_WhenNoInScopeKpis()
        {
            // No KPIs at all.
            Assert.Equal(ReadinessState.NotConfigured,
                ReadinessService.Evaluate(Array.Empty<KpiDefinition>(), Array.Empty<KpiResult>()));

            // KPIs exist but none are in meeting scope: a retired one and a live-out-of-scope one.
            var kpis = new[] { Kpi(1, active: false, inScope: true), Kpi(2, active: true, inScope: false) };
            Assert.Equal(ReadinessState.NotConfigured,
                ReadinessService.Evaluate(kpis, Array.Empty<KpiResult>()));
        }

        [Fact]
        public void NotReady_WhenSomeInScopeKpiLacksAResultThisWeek()
        {
            var kpis = new[] { Kpi(1), Kpi(2) };
            // Only KPI 1 has a result this Week; KPI 2 is missing.
            var results = new[] { Result(1) };

            Assert.Equal(ReadinessState.NotReady, ReadinessService.Evaluate(kpis, results));
        }

        [Fact]
        public void Ready_WhenEveryInScopeKpiHasAResult_RedCounts()
        {
            var kpis = new[] { Kpi(1), Kpi(2) };
            // Every in-scope KPI reported; a Red result still counts as reported.
            var results = new[] { Result(1, KpiStatus.Green), Result(2, KpiStatus.Red) };

            Assert.Equal(ReadinessState.Ready, ReadinessService.Evaluate(kpis, results));
        }

        [Fact]
        public void OutOfScopeAndRetiredKpis_DoNotAffectReadiness()
        {
            // One in-scope KPI (reported) plus an out-of-scope and a retired KPI with no results.
            var kpis = new[]
            {
                Kpi(1, active: true, inScope: true),
                Kpi(2, active: true, inScope: false),
                Kpi(3, active: false, inScope: true)
            };
            var results = new[] { Result(1) };

            // Only the in-scope KPI matters, and it is reported, so the Team is Ready.
            Assert.Equal(ReadinessState.Ready, ReadinessService.Evaluate(kpis, results));
        }
    }
}
