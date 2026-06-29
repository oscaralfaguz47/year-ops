using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Covers the pure KPI Review surfacing predicate: a KPI surfaces iff it is in
    /// meeting scope (Active &amp;&amp; InScope); among those, a non-Green or missing
    /// result is Loud while Green stays Quiet. Out-of-scope KPIs are Hidden regardless
    /// of status. No DbContext/HttpContext — operates on a definition + optional result.
    /// </summary>
    public class KpiSurfacingTests
    {
        private static KpiDefinition Kpi(bool active = true, bool inScope = true) =>
            new() { Name = "On-time delivery", Target = ">= 95%", Active = active, InScope = inScope };

        private static KpiResult Result(KpiStatus status) =>
            new() { Value = "92%", Status = status };

        [Fact]
        public void OutOfScopeKpi_IsHidden_RegardlessOfStatus()
        {
            var outOfScope = Kpi(active: true, inScope: false);

            Assert.Equal(KpiSurfacing.Hidden, ReviewSurfacingService.SurfaceKpi(outOfScope, Result(KpiStatus.Red)));
            Assert.Equal(KpiSurfacing.Hidden, ReviewSurfacingService.SurfaceKpi(outOfScope, Result(KpiStatus.Green)));
            Assert.Equal(KpiSurfacing.Hidden, ReviewSurfacingService.SurfaceKpi(outOfScope, result: null));
        }

        [Fact]
        public void RetiredKpi_IsHidden_EvenWhenInScope()
        {
            var retired = Kpi(active: false, inScope: true);

            Assert.Equal(KpiSurfacing.Hidden, ReviewSurfacingService.SurfaceKpi(retired, Result(KpiStatus.Red)));
        }

        [Theory]
        [InlineData(KpiStatus.Red)]
        [InlineData(KpiStatus.Yellow)]
        public void InScopeNonGreenKpi_IsLoud(KpiStatus status)
        {
            Assert.Equal(KpiSurfacing.Loud, ReviewSurfacingService.SurfaceKpi(Kpi(), Result(status)));
        }

        [Fact]
        public void InScopeMissingResultKpi_IsLoud()
        {
            Assert.Equal(KpiSurfacing.Loud, ReviewSurfacingService.SurfaceKpi(Kpi(), result: null));
        }

        [Fact]
        public void InScopeGreenKpi_IsQuiet()
        {
            Assert.Equal(KpiSurfacing.Quiet, ReviewSurfacingService.SurfaceKpi(Kpi(), Result(KpiStatus.Green)));
        }
    }
}
