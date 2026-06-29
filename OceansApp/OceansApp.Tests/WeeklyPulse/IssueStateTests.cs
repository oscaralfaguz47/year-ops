using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Covers the pure stateAsOf selector: the latest status row with WeekStart &lt;= w.
    /// No DbContext/HttpContext — operates on plain history rows.
    /// </summary>
    public class IssueStateTests
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

        [Fact]
        public void StateAsOf_ReturnsLatestStatusRow_OnOrBeforeWeek()
        {
            var history = new[]
            {
                Status(IssueStatus.Open, W1),
                Status(IssueStatus.Deferred, W2),
                Status(IssueStatus.Solved, W3),
            };

            Assert.Equal(IssueStatus.Open, IssueStateService.StateAsOf(history, W1));
            Assert.Equal(IssueStatus.Deferred, IssueStateService.StateAsOf(history, W2));
            Assert.Equal(IssueStatus.Solved, IssueStateService.StateAsOf(history, W3));
        }

        [Fact]
        public void StateAsOf_HoldsLastStatus_ForWeeksWithNoChange()
        {
            // Open in W1, Solved in W3 — the in-between W2 still reads as Open,
            // and any later week keeps the terminal Solved.
            var history = new[]
            {
                Status(IssueStatus.Open, W1),
                Status(IssueStatus.Solved, W3),
            };

            Assert.Equal(IssueStatus.Open, IssueStateService.StateAsOf(history, W2));
            Assert.Equal(IssueStatus.Solved, IssueStateService.StateAsOf(history, new DateOnly(2026, 7, 6)));
        }

        [Fact]
        public void StateAsOf_DefaultsToOpen_ForWeeksBeforeAnyStatusRow()
        {
            var history = new[] { Status(IssueStatus.Open, W2) };

            Assert.Equal(IssueStatus.Open, IssueStateService.StateAsOf(history, W1));
        }

        [Fact]
        public void StateAsOf_IgnoresCommentRows()
        {
            var history = new[]
            {
                Status(IssueStatus.Open, W1),
                new IssueHistory
                {
                    ChangeType = IssueChangeType.Comment,
                    Comment = "Identified: enterprise pricing unclear",
                    WeekStart = W2,
                    CreatedAt = new DateTimeOffset(W2.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
                },
            };

            // The comment in W2 does not move state off Open.
            Assert.Equal(IssueStatus.Open, IssueStateService.StateAsOf(history, W2));
        }

        [Fact]
        public void StateAsOf_UsesLatestWithinSameWeek_ByCreatedAtThenInsertionOrder()
        {
            // Two transitions in the same Week: Open then Deferred. The later one wins.
            var history = new[]
            {
                Status(IssueStatus.Open, W1, seq: 0),
                Status(IssueStatus.Deferred, W1, seq: 1),
            };

            Assert.Equal(IssueStatus.Deferred, IssueStateService.StateAsOf(history, W1));
        }
    }
}
