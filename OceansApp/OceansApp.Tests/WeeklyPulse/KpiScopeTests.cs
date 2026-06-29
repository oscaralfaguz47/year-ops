using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Pure-logic tests for <see cref="KpiScopeService"/>: the two flags are independent,
    /// and retiring a KPI (Active=false) drops it from both Readiness and the Review.
    /// </summary>
    public class KpiScopeTests
    {
        private static KpiDefinition Kpi(bool active, bool inScope) =>
            new() { Name = "On-time delivery", Target = ">= 95%", Active = active, InScope = inScope };

        [Fact]
        public void ActiveKpi_ExpectsInput_ForReadiness()
        {
            Assert.True(KpiScopeService.ExpectsInput(Kpi(active: true, inScope: true)));
            Assert.True(KpiScopeService.ExpectsInput(Kpi(active: true, inScope: false)));
        }

        [Fact]
        public void RetiredKpi_DropsFromReadiness_AndReview()
        {
            var retired = Kpi(active: false, inScope: true);

            // Retiring stops it expecting new input (Readiness) and removes it from the meeting.
            Assert.False(KpiScopeService.ExpectsInput(retired));
            Assert.False(KpiScopeService.SurfacesInReview(retired));
        }

        [Fact]
        public void SurfacesInReview_RequiresBothActiveAndInScope()
        {
            Assert.True(KpiScopeService.SurfacesInReview(Kpi(active: true, inScope: true)));
            Assert.False(KpiScopeService.SurfacesInReview(Kpi(active: true, inScope: false)));
            Assert.False(KpiScopeService.SurfacesInReview(Kpi(active: false, inScope: true)));
        }

        [Fact]
        public void Flags_AreIndependent()
        {
            // Active without InScope: live and expecting input, but out of the meeting.
            var liveOutOfScope = Kpi(active: true, inScope: false);
            Assert.True(KpiScopeService.ExpectsInput(liveOutOfScope));
            Assert.False(KpiScopeService.SurfacesInReview(liveOutOfScope));

            // InScope without Active is a no-op for surfacing — retired always drops out.
            var retiredInScope = Kpi(active: false, inScope: true);
            Assert.False(KpiScopeService.ExpectsInput(retiredInScope));
            Assert.False(KpiScopeService.SurfacesInReview(retiredInScope));
        }
    }
}
