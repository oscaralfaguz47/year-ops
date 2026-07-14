using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Covers the pure KPI Review surfacing predicate: this Week's result surfaces iff it is
    /// included (IncludeInReview); among those, a non-Green result is Loud while Green stays
    /// Quiet. A result that is un-ticked — or a missing result — is Hidden. No DbContext/
    /// HttpContext — operates on an optional result.
    /// </summary>
    public class KpiSurfacingTests
    {
        private static KpiResult Result(KpiStatus status, bool includeInReview = true) =>
            new() { Value = "92%", Status = status, IncludeInReview = includeInReview };

        [Fact]
        public void ExcludedResult_IsHidden_RegardlessOfStatus()
        {
            Assert.Equal(KpiSurfacing.Hidden, ReviewSurfacingService.SurfaceKpi(Result(KpiStatus.Red, includeInReview: false)));
            Assert.Equal(KpiSurfacing.Hidden, ReviewSurfacingService.SurfaceKpi(Result(KpiStatus.Green, includeInReview: false)));
        }

        [Fact]
        public void MissingResult_IsHidden()
        {
            Assert.Equal(KpiSurfacing.Hidden, ReviewSurfacingService.SurfaceKpi(result: null));
        }

        [Theory]
        [InlineData(KpiStatus.Red)]
        [InlineData(KpiStatus.Yellow)]
        public void IncludedNonGreenResult_IsLoud(KpiStatus status)
        {
            Assert.Equal(KpiSurfacing.Loud, ReviewSurfacingService.SurfaceKpi(Result(status)));
        }

        [Fact]
        public void IncludedGreenResult_IsQuiet()
        {
            Assert.Equal(KpiSurfacing.Quiet, ReviewSurfacingService.SurfaceKpi(Result(KpiStatus.Green)));
        }
    }
}
