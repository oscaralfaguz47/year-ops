using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Conversion rules (WP-2.6): each conversion produces a <b>pre-filled</b> living
    /// entity carrying an origin back-reference (origin type + id). These are pure
    /// mapping rules — they never touch the source, which the repository preserves.
    /// </summary>
    public class ConversionServiceTests
    {
        private static readonly DateOnly SourceWeek = new(2026, 6, 1);
        private static readonly DateOnly ConversionWeek = new(2026, 6, 15);
        private static readonly DateOnly Due = new(2026, 6, 30);

        [Fact]
        public void FromCheckIn_PrefillsIssue_WithCheckInOrigin()
        {
            var checkIn = new CheckIn
            {
                CheckInId = 7,
                TeamId = 3,
                WeekStart = SourceWeek,
                Type = CheckInType.Concern,
                Note = "Vendor invoice is late again"
            };

            var issue = ConversionService.FromCheckIn(checkIn, ConversionWeek);

            Assert.Equal(3, issue.TeamId);
            Assert.Equal("[from check-in] Vendor invoice is late again", issue.Title);
            Assert.Equal(IssuePriority.Med, issue.Priority);
            // Raised in the conversion Week, not the source's Week.
            Assert.Equal(ConversionWeek, issue.OriginWeekStart);
            Assert.Equal(OriginType.CheckIn, issue.OriginType);
            Assert.Equal(7, issue.OriginId);
        }

        [Fact]
        public void FromHeadline_PrefillsIssue_RiskMapsToHighPriority()
        {
            var headline = new Headline
            {
                HeadlineId = 12,
                TeamId = 4,
                WeekStart = SourceWeek,
                Type = HeadlineType.Risk,
                Text = "Key client threatening to churn"
            };

            var issue = ConversionService.FromHeadline(headline, ConversionWeek);

            Assert.Equal(4, issue.TeamId);
            Assert.Equal("[from headline] Key client threatening to churn", issue.Title);
            Assert.Equal(IssuePriority.High, issue.Priority);
            Assert.Equal(ConversionWeek, issue.OriginWeekStart);
            Assert.Equal(OriginType.Headline, issue.OriginType);
            Assert.Equal(12, issue.OriginId);
        }

        [Fact]
        public void FromHeadline_HighlightMapsToMedPriority()
        {
            var headline = new Headline
            {
                HeadlineId = 13,
                TeamId = 4,
                WeekStart = SourceWeek,
                Type = HeadlineType.Highlight,
                Text = "Shipped the new onboarding flow"
            };

            var issue = ConversionService.FromHeadline(headline, ConversionWeek);

            Assert.Equal(IssuePriority.Med, issue.Priority);
            Assert.Equal(OriginType.Headline, issue.OriginType);
            Assert.Equal(13, issue.OriginId);
        }

        [Fact]
        public void FromIssue_PrefillsToDo_WithIssueOrigin_AndProvidedOwnerAndDue()
        {
            var issue = new Issue
            {
                IssueId = 21,
                TeamId = 5,
                Title = "Decide on new pricing tiers",
                Priority = IssuePriority.High,
                OriginWeekStart = SourceWeek
            };

            var toDo = ConversionService.FromIssue(issue, "owner-9", Due, ConversionWeek);

            Assert.Equal(5, toDo.TeamId);
            Assert.Equal("[from issue] Decide on new pricing tiers", toDo.Title);
            Assert.Equal("owner-9", toDo.OwnerId);
            Assert.Equal(Due, toDo.DueDate);
            Assert.Equal(ConversionWeek, toDo.OriginWeekStart);
            Assert.Equal(OriginType.Issue, toDo.OriginType);
            Assert.Equal(21, toDo.OriginId);
        }
    }
}
