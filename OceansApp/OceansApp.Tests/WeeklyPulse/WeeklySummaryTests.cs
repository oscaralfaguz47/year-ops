using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Covers the pure Weekly Summary derivation: an auto-assembled, READ-ONLY draft
    /// computed on the fly from a Week's data (never a stored entity — see ADR 0001).
    /// Decisions &lt;- Issues Solved this Week; Actions &lt;- To-Dos created in-meeting this
    /// Week; Risks &lt;- Risk-type Headlines + High/Critical open Issues; and a single
    /// suggested-format summary sentence. No DbContext/HttpContext — operates on plain
    /// Issue/To-Do/Headline rows.
    /// </summary>
    public class WeeklySummaryTests
    {
        private static readonly DateOnly W1 = new(2026, 6, 1);
        private static readonly DateOnly W2 = new(2026, 6, 8);
        private static readonly DateOnly W3 = new(2026, 6, 15);

        private static IssueHistory Status(IssueStatus status, DateOnly week, int seq = 0) =>
            new()
            {
                ChangeType = IssueChangeType.Status,
                Status = status,
                WeekStart = week,
                CreatedAt = new DateTimeOffset(week.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).AddMinutes(seq)
            };

        private static Issue Issue(string title, IssuePriority priority, params IssueHistory[] history) =>
            new() { Title = title, Priority = priority, History = history };

        private static ToDo ToDo(string title, DateOnly originWeek) =>
            new() { Title = title, OriginWeekStart = originWeek, DueDate = originWeek };

        private static Headline Headline(string text, HeadlineType type, DateOnly week) =>
            new() { Text = text, Type = type, WeekStart = week };

        // ---- Decisions <- Issues Solved this Week --------------------------

        [Fact]
        public void Decisions_AreIssuesSolvedThisWeek()
        {
            var solvedThisWeek = Issue("Pricing decided", IssuePriority.High,
                Status(IssueStatus.Open, W1), Status(IssueStatus.Solved, W2));
            var solvedLastWeek = Issue("Old call", IssuePriority.Med,
                Status(IssueStatus.Solved, W1));
            var stillOpen = Issue("Unresolved", IssuePriority.Low,
                Status(IssueStatus.Open, W2));

            var summary = WeeklySummaryService.Derive(
                new[] { solvedThisWeek, solvedLastWeek, stillOpen },
                Array.Empty<ToDo>(), Array.Empty<Headline>(), W2);

            var decision = Assert.Single(summary.Decisions);
            Assert.Equal("Pricing decided", decision.Title);
        }

        [Fact]
        public void Decisions_ExcludeIssuesReopenedAfterBeingSolvedThisWeek()
        {
            // Solved then re-opened in the same Week — the determining row is the re-open,
            // so it is not a decision.
            var churned = Issue("Churned", IssuePriority.High,
                Status(IssueStatus.Solved, W2, seq: 0), Status(IssueStatus.Open, W2, seq: 1));

            var summary = WeeklySummaryService.Derive(
                new[] { churned }, Array.Empty<ToDo>(), Array.Empty<Headline>(), W2);

            Assert.Empty(summary.Decisions);
        }

        // ---- Actions <- To-Dos created in-meeting this Week ----------------

        [Fact]
        public void Actions_AreToDosCreatedThisWeek()
        {
            var thisWeek = ToDo("Draft the deck", W2);
            var earlier = ToDo("Carried over", W1);

            var summary = WeeklySummaryService.Derive(
                Array.Empty<Issue>(), new[] { thisWeek, earlier }, Array.Empty<Headline>(), W2);

            var action = Assert.Single(summary.Actions);
            Assert.Equal("Draft the deck", action.Title);
        }

        // ---- Risks <- Risk-type Headlines + High/Critical open Issues ------

        [Fact]
        public void Risks_IncludeRiskTypeHeadlinesPostedThisWeek()
        {
            var risk = Headline("Vendor may slip", HeadlineType.Risk, W2);
            var win = Headline("Shipped early", HeadlineType.Highlight, W2);
            var oldRisk = Headline("Last week's risk", HeadlineType.Risk, W1);

            var summary = WeeklySummaryService.Derive(
                Array.Empty<Issue>(), Array.Empty<ToDo>(), new[] { risk, win, oldRisk }, W2);

            var headline = Assert.Single(summary.RiskHeadlines);
            Assert.Equal("Vendor may slip", headline.Text);
        }

        [Fact]
        public void Risks_IncludeHighAndCriticalOpenIssues_ExcludeLowMedAndNonOpen()
        {
            var criticalOpen = Issue("Critical open", IssuePriority.Critical, Status(IssueStatus.Open, W1));
            var highOpen = Issue("High open", IssuePriority.High, Status(IssueStatus.Open, W2));
            var medOpen = Issue("Med open", IssuePriority.Med, Status(IssueStatus.Open, W1));
            var highSolved = Issue("High solved", IssuePriority.High,
                Status(IssueStatus.Open, W1), Status(IssueStatus.Solved, W2));

            var summary = WeeklySummaryService.Derive(
                new[] { criticalOpen, highOpen, medOpen, highSolved },
                Array.Empty<ToDo>(), Array.Empty<Headline>(), W2);

            // Critical first (priority order), then High; Med and the Solved one are excluded.
            Assert.Equal(new[] { "Critical open", "High open" },
                summary.RiskIssues.Select(i => i.Title));
        }

        // ---- Summary text <- the suggested-format sentence -----------------

        [Fact]
        public void SummaryText_IsTheSuggestedFormatSentence()
        {
            var decision = Issue("Decided", IssuePriority.Med, Status(IssueStatus.Solved, W2));
            var action = ToDo("Do it", W2);
            var risk = Headline("Risky", HeadlineType.Risk, W2);
            var criticalOpen = Issue("Critical", IssuePriority.Critical, Status(IssueStatus.Open, W2));

            var summary = WeeklySummaryService.Derive(
                new[] { decision, criticalOpen }, new[] { action }, new[] { risk }, W2);

            // 1 decision, 1 action, 2 risks (1 risk headline + 1 High/Critical open issue).
            Assert.Equal(
                "Week of 2026-06-08: 1 decision(s) made, 1 action(s) created, 2 risk(s) flagged.",
                summary.SummaryText);
        }

        [Fact]
        public void Derive_OnEmptyWeek_YieldsAllEmptyAndZeroedSentence()
        {
            var summary = WeeklySummaryService.Derive(
                Array.Empty<Issue>(), Array.Empty<ToDo>(), Array.Empty<Headline>(), W3);

            Assert.Empty(summary.Decisions);
            Assert.Empty(summary.Actions);
            Assert.Empty(summary.RiskHeadlines);
            Assert.Empty(summary.RiskIssues);
            Assert.Equal(
                "Week of 2026-06-15: 0 decision(s) made, 0 action(s) created, 0 risk(s) flagged.",
                summary.SummaryText);
        }
    }
}
