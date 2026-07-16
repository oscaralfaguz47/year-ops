using OceansApp.Models.Domain.WeeklyPulse;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Covers the guided sequential Review script (WP-CR4): the Review is an ordered
    /// per-Team walk through fixed moments — Check-in -> KPI -> Headlines -> Issues ->
    /// To-Dos — each with its own edit rule. This is pure presentation contract (no
    /// DbContext/HttpContext), the single source of truth the Review view walks.
    /// </summary>
    public class ReviewScriptTests
    {
        [Fact]
        public void Moments_AreTheFixedGuidedWalk_InOrder()
        {
            Assert.Equal(
                new[]
                {
                    ReviewMoment.CheckIn,
                    ReviewMoment.Kpi,
                    ReviewMoment.Headlines,
                    ReviewMoment.Issues,
                    ReviewMoment.ToDos
                },
                ReviewScriptService.Moments);
        }

        [Theory]
        // Only Issues and To-Dos are editable live.
        [InlineData(ReviewMoment.CheckIn, false)]
        [InlineData(ReviewMoment.Kpi, false)]
        [InlineData(ReviewMoment.Headlines, false)]
        [InlineData(ReviewMoment.Issues, true)]
        [InlineData(ReviewMoment.ToDos, true)]
        public void IsEditable_OnlyForIssuesAndToDos(ReviewMoment moment, bool editable)
        {
            Assert.Equal(editable, ReviewScriptService.IsEditable(moment));
        }

        [Theory]
        // The Headlines moment's only action is dropping a headline to an Issue.
        [InlineData(ReviewMoment.CheckIn, false)]
        [InlineData(ReviewMoment.Kpi, false)]
        [InlineData(ReviewMoment.Headlines, true)]
        [InlineData(ReviewMoment.Issues, false)]
        [InlineData(ReviewMoment.ToDos, false)]
        public void CanDropToIssue_OnlyForHeadlines(ReviewMoment moment, bool canDrop)
        {
            Assert.Equal(canDrop, ReviewScriptService.CanDropToIssue(moment));
        }

        [Theory]
        // Issue -> To-Do is available on the Issues moment only; a Headline never
        // spawns a To-Do directly (it can only become an Issue first).
        [InlineData(ReviewMoment.CheckIn, false)]
        [InlineData(ReviewMoment.Kpi, false)]
        [InlineData(ReviewMoment.Headlines, false)]
        [InlineData(ReviewMoment.Issues, true)]
        [InlineData(ReviewMoment.ToDos, false)]
        public void CanSpawnToDo_OnlyForIssues(ReviewMoment moment, bool canSpawn)
        {
            Assert.Equal(canSpawn, ReviewScriptService.CanSpawnToDo(moment));
        }
    }
}
