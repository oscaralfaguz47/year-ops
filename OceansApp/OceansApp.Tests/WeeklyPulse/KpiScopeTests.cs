using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Pure-logic tests for <see cref="KpiScopeService"/>: <see cref="KpiScopeService.ExpectsInput"/>
    /// is driven by the definition's Active flag (drives Readiness), while
    /// <see cref="KpiScopeService.SurfacesInReview"/> is a per-Week decision on the result
    /// (IncludeInReview). Retiring a KPI drops it from Readiness; un-ticking a result — or having
    /// no result this Week — drops it from the Review, and the two are independent.
    /// </summary>
    public class KpiScopeTests
    {
        private static KpiDefinition Kpi(bool active) =>
            new() { Name = "On-time delivery", Target = ">= 95%", Active = active };

        private static KpiResult Result(bool includeInReview) =>
            new() { Value = "92%", Status = KpiStatus.Green, IncludeInReview = includeInReview };

        [Fact]
        public void ActiveKpi_ExpectsInput_ForReadiness()
        {
            Assert.True(KpiScopeService.ExpectsInput(Kpi(active: true)));
        }

        [Fact]
        public void RetiredKpi_DropsFromReadiness()
        {
            // Retiring stops it expecting new input (Readiness); it no longer counts.
            Assert.False(KpiScopeService.ExpectsInput(Kpi(active: false)));
        }

        [Fact]
        public void SurfacesInReview_RequiresAnIncludedResultThisWeek()
        {
            Assert.True(KpiScopeService.SurfacesInReview(Result(includeInReview: true)));
            Assert.False(KpiScopeService.SurfacesInReview(Result(includeInReview: false)));
        }

        [Fact]
        public void MissingResult_DoesNotSurfaceInReview()
        {
            Assert.False(KpiScopeService.SurfacesInReview(result: null));
        }

        [Fact]
        public void ReviewInclusion_IsIndependentOfReadiness()
        {
            // An Active KPI counts toward Readiness regardless of whether its result is included.
            var active = Kpi(active: true);
            Assert.True(KpiScopeService.ExpectsInput(active));
            Assert.False(KpiScopeService.SurfacesInReview(Result(includeInReview: false)));
        }
    }
}
