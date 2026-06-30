using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Covers the pure stateAsOf selector for a To-Do: the latest status row with
    /// WeekStart &lt;= w. No DbContext/HttpContext — operates on plain history rows.
    /// Mirrors <see cref="IssueStateTests"/>.
    /// </summary>
    public class ToDoStateTests
    {
        private static readonly DateOnly W1 = new(2026, 6, 1);
        private static readonly DateOnly W2 = new(2026, 6, 8);
        private static readonly DateOnly W3 = new(2026, 6, 15);

        private static ToDoHistory Status(ToDoStatus status, DateOnly week, int seq = 0) =>
            new()
            {
                ChangeType = ToDoChangeType.Status,
                Status = status,
                WeekStart = week,
                CreatedAt = new DateTimeOffset(week.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).AddMinutes(seq)
            };

        [Fact]
        public void StateAsOf_ReturnsLatestStatusRow_OnOrBeforeWeek()
        {
            var history = new[]
            {
                Status(ToDoStatus.Open, W1),
                Status(ToDoStatus.Blocked, W2),
                Status(ToDoStatus.Done, W3),
            };

            Assert.Equal(ToDoStatus.Open, ToDoStateService.StateAsOf(history, W1));
            Assert.Equal(ToDoStatus.Blocked, ToDoStateService.StateAsOf(history, W2));
            Assert.Equal(ToDoStatus.Done, ToDoStateService.StateAsOf(history, W3));
        }

        [Fact]
        public void StateAsOf_HoldsLastStatus_ForWeeksWithNoChange()
        {
            var history = new[]
            {
                Status(ToDoStatus.Open, W1),
                Status(ToDoStatus.Done, W3),
            };

            Assert.Equal(ToDoStatus.Open, ToDoStateService.StateAsOf(history, W2));
            Assert.Equal(ToDoStatus.Done, ToDoStateService.StateAsOf(history, new DateOnly(2026, 7, 6)));
        }

        [Fact]
        public void StateAsOf_DefaultsToOpen_ForWeeksBeforeAnyStatusRow()
        {
            var history = new[] { Status(ToDoStatus.Open, W2) };

            Assert.Equal(ToDoStatus.Open, ToDoStateService.StateAsOf(history, W1));
        }

        [Fact]
        public void StateAsOf_IgnoresCommentRows()
        {
            var history = new[]
            {
                Status(ToDoStatus.Open, W1),
                new ToDoHistory
                {
                    ChangeType = ToDoChangeType.Comment,
                    Comment = "Waiting on vendor reply",
                    WeekStart = W2,
                    CreatedAt = new DateTimeOffset(W2.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
                },
            };

            Assert.Equal(ToDoStatus.Open, ToDoStateService.StateAsOf(history, W2));
        }

        [Fact]
        public void StateAsOf_UsesLatestWithinSameWeek_ByCreatedAtThenInsertionOrder()
        {
            var history = new[]
            {
                Status(ToDoStatus.Open, W1, seq: 0),
                Status(ToDoStatus.Blocked, W1, seq: 1),
            };

            Assert.Equal(ToDoStatus.Blocked, ToDoStateService.StateAsOf(history, W1));
        }
    }
}
