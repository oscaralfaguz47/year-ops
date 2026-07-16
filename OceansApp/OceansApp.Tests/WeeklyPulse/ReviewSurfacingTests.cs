using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Covers the pure Review surfacing predicate (ported from prototype model.js
    /// reviewSurfacing): an Issue surfaces iff (state == Open || pinned) &amp;&amp;
    /// state != Solved. No DbContext/HttpContext — operates on a state + pin flag.
    /// Also covers the pin guard: only Deferred issues are pinnable.
    /// </summary>
    public class ReviewSurfacingTests
    {
        [Fact]
        public void OpenIssue_Surfaces_WhetherOrNotPinned()
        {
            Assert.True(ReviewSurfacingService.Surfaces(IssueStatus.Open, pinned: false));
            Assert.True(ReviewSurfacingService.Surfaces(IssueStatus.Open, pinned: true));
        }

        [Fact]
        public void PinnedDeferredIssue_Surfaces()
        {
            Assert.True(ReviewSurfacingService.Surfaces(IssueStatus.Deferred, pinned: true));
        }

        [Fact]
        public void UnpinnedDeferredIssue_IsHidden()
        {
            Assert.False(ReviewSurfacingService.Surfaces(IssueStatus.Deferred, pinned: false));
        }

        [Fact]
        public void SolvedIssue_NeverSurfaces_EvenWhenPinned()
        {
            Assert.False(ReviewSurfacingService.Surfaces(IssueStatus.Solved, pinned: false));
            Assert.False(ReviewSurfacingService.Surfaces(IssueStatus.Solved, pinned: true));
        }

        [Theory]
        [InlineData(IssueStatus.Open, false)]
        [InlineData(IssueStatus.Solved, false)]
        public void EnsurePinnable_RejectsNonDeferred(IssueStatus state, bool _)
        {
            Assert.Throws<InvalidOperationException>(() => ReviewSurfacingService.EnsurePinnable(state));
        }

        [Fact]
        public void EnsurePinnable_AllowsDeferred()
        {
            // does not throw
            ReviewSurfacingService.EnsurePinnable(IssueStatus.Deferred);
        }
    }
}
