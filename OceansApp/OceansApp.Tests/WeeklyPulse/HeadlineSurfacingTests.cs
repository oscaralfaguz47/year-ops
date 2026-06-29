using System.Reflection;
using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Covers the pure Headline Review surfacing rule: the news round — <i>every</i>
    /// headline surfaces (there is no scope gate and no quiet/hidden state), with Risk
    /// flagged Loud and Highlight kept Quiet. Also locks the decision that headlines are
    /// <b>not pinnable</b> (every headline already surfaces, so there is nothing to pin).
    /// No DbContext/HttpContext — operates on a headline type.
    /// </summary>
    public class HeadlineSurfacingTests
    {
        [Fact]
        public void RiskHeadline_IsLoud()
        {
            Assert.Equal(HeadlineEmphasis.Loud, ReviewSurfacingService.SurfaceHeadline(HeadlineType.Risk));
        }

        [Fact]
        public void HighlightHeadline_IsQuiet()
        {
            Assert.Equal(HeadlineEmphasis.Quiet, ReviewSurfacingService.SurfaceHeadline(HeadlineType.Highlight));
        }

        [Theory]
        [InlineData(HeadlineType.Highlight)]
        [InlineData(HeadlineType.Risk)]
        public void EveryHeadline_Surfaces_InTheNewsRound(HeadlineType type)
        {
            // Unlike KPIs/issues there is no Hidden/Quiet-skip outcome: every headline of
            // every type surfaces. Loud and Quiet are both "shown", just emphasised differently.
            var emphasis = ReviewSurfacingService.SurfaceHeadline(type);
            Assert.True(emphasis is HeadlineEmphasis.Loud or HeadlineEmphasis.Quiet);
        }

        [Fact]
        public void Headline_IsNotPinnable_CarriesNoPinFlag()
        {
            // Headlines are not pinnable — they all already surface in the news round — so
            // the entity deliberately carries no pin flag (contrast with Issue.Pinned).
            Assert.Null(typeof(Headline).GetProperty("Pinned", BindingFlags.Public | BindingFlags.Instance));
        }
    }
}
