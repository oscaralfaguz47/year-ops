using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Covers the pure Meeting History (minutes) derivations: <c>activeInWeek</c>
    /// (surfaced-in-Review OR commented/status-changed that Week) and the past-Week
    /// enumeration (distinct WeekStart values across the data). No DbContext/HttpContext —
    /// operates on plain history rows and week stamps.
    /// </summary>
    public class MeetingHistoryTests
    {
        private static readonly DateOnly W1 = new(2026, 6, 1);
        private static readonly DateOnly W2 = new(2026, 6, 8);
        private static readonly DateOnly W3 = new(2026, 6, 15);

        private static IssueHistory IssueStatusRow(IssueStatus status, DateOnly week, int seq = 0) =>
            new()
            {
                ChangeType = IssueChangeType.Status,
                Status = status,
                WeekStart = week,
                CreatedAt = new DateTimeOffset(week.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).AddMinutes(seq)
            };

        private static IssueHistory IssueCommentRow(DateOnly week, int seq = 0) =>
            new()
            {
                ChangeType = IssueChangeType.Comment,
                Comment = "IDS note",
                WeekStart = week,
                CreatedAt = new DateTimeOffset(week.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).AddMinutes(seq)
            };

        private static ToDoHistory ToDoStatusRow(ToDoStatus status, DateOnly week, int seq = 0) =>
            new()
            {
                ChangeType = ToDoChangeType.Status,
                Status = status,
                WeekStart = week,
                CreatedAt = new DateTimeOffset(week.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).AddMinutes(seq)
            };

        // ---- IssueActiveInWeek ---------------------------------------------

        [Fact]
        public void Issue_AppearsUnderEachWeekItTouched_WithItsAsOfState()
        {
            // Raised Open in W1, Deferred in W2 (not pinned). It is active in BOTH weeks:
            // W1 because an Open issue surfaces in Review, W2 because it had a status row
            // that Week — each shown with its as-of state (Open in W1, Deferred in W2).
            var history = new[]
            {
                IssueStatusRow(IssueStatus.Open, W1),
                IssueStatusRow(IssueStatus.Deferred, W2),
            };

            Assert.True(MeetingHistoryService.IssueActiveInWeek(history, originWeekStart: W1, W1));
            Assert.True(MeetingHistoryService.IssueActiveInWeek(history, originWeekStart: W1, W2));

            Assert.Equal(IssueStatus.Open, IssueStateService.StateAsOf(history, W1));
            Assert.Equal(IssueStatus.Deferred, IssueStateService.StateAsOf(history, W2));
        }

        [Fact]
        public void IssueActiveInWeek_True_WhenSurfacesInReview_EvenWithNoRowThatWeek()
        {
            // Open in W1, nothing in W2: state holds Open, so it surfaces in W2's Review
            // and is therefore active in W2 despite having no row that Week.
            var history = new[] { IssueStatusRow(IssueStatus.Open, W1) };

            Assert.True(MeetingHistoryService.IssueActiveInWeek(history, originWeekStart: W1, W2));
        }

        [Fact]
        public void IssueActiveInWeek_False_BeforeItExisted_DespiteStateAsOfDefaultingToOpen()
        {
            // Raised Open in W2 (origin = W2). In W1, before it existed, StateAsOf defaults
            // to Open (no row on/before W1) which would surface it — the origin guard must
            // keep it out of pre-origin Weeks.
            var history = new[] { IssueStatusRow(IssueStatus.Open, W2) };

            Assert.Equal(IssueStatus.Open, IssueStateService.StateAsOf(history, W1)); // the trap
            Assert.False(MeetingHistoryService.IssueActiveInWeek(history, originWeekStart: W2, W1));
            Assert.True(MeetingHistoryService.IssueActiveInWeek(history, originWeekStart: W2, W2));
        }

        [Fact]
        public void IssueActiveInWeek_True_WhenCommentedThatWeek_ThoughQuietInReview()
        {
            // Deferred (parked, unpinned) so it does NOT surface in Review, but a comment
            // row in W3 means it was touched that Week — active.
            var history = new[]
            {
                IssueStatusRow(IssueStatus.Deferred, W2),
                IssueCommentRow(W3),
            };

            Assert.True(MeetingHistoryService.IssueActiveInWeek(history, originWeekStart: W2, W3));
        }

        [Fact]
        public void IssueActiveInWeek_False_WhenParkedQuietAndUntouchedThatWeek()
        {
            // Deferred in W2, no row in W3: neither surfaces nor touched -> not active.
            var history = new[] { IssueStatusRow(IssueStatus.Deferred, W2) };

            Assert.False(MeetingHistoryService.IssueActiveInWeek(history, originWeekStart: W2, W3));
        }

        [Fact]
        public void IssueActiveInWeek_True_InTheWeekItWasSolved_ThenFalseAfter()
        {
            // Solved in W3: active in W3 (status row that Week) but not in a later, untouched
            // week — a Solved issue never surfaces.
            var history = new[] { IssueStatusRow(IssueStatus.Solved, W3) };

            Assert.True(MeetingHistoryService.IssueActiveInWeek(history, originWeekStart: W3, W3));
            Assert.False(MeetingHistoryService.IssueActiveInWeek(history, originWeekStart: W3, new DateOnly(2026, 6, 22)));
        }

        [Fact]
        public void IssueActiveInWeek_IgnoresLivePin_SoTogglingItCannotRewritePastMinutes()
        {
            // A Deferred issue untouched in W3 is quiet in Review. The live pin is a mutable
            // current-meeting flag (not week-stamped), so it must NOT make the issue active
            // in past minutes — otherwise toggling the pin today would rewrite W3's record.
            var history = new[] { IssueStatusRow(IssueStatus.Deferred, W2) };

            Assert.False(MeetingHistoryService.IssueActiveInWeek(history, originWeekStart: W2, W3));
        }

        // ---- ToDoActiveInWeek ----------------------------------------------

        [Fact]
        public void ToDoActiveInWeek_True_WhenNonDoneSurfaces_AndInWeekDone_ThenFalseAfter()
        {
            // Open in W1 (surfaces every week until Done), Done in W3 (status row that Week
            // -> active), then not active in a later untouched week.
            var history = new[]
            {
                ToDoStatusRow(ToDoStatus.Open, W1),
                ToDoStatusRow(ToDoStatus.Done, W3),
            };

            Assert.True(MeetingHistoryService.ToDoActiveInWeek(history, originWeekStart: W1, W2));
            Assert.True(MeetingHistoryService.ToDoActiveInWeek(history, originWeekStart: W1, W3));
            Assert.False(MeetingHistoryService.ToDoActiveInWeek(history, originWeekStart: W1, new DateOnly(2026, 6, 22)));
        }

        [Fact]
        public void ToDoActiveInWeek_False_BeforeItExisted_DespiteStateAsOfDefaultingToOpen()
        {
            // Created Open in W2 (origin = W2). In W1, before it existed, StateAsOf defaults
            // to Open (which surfaces) — the origin guard must keep it out of pre-origin Weeks.
            var history = new[] { ToDoStatusRow(ToDoStatus.Open, W2) };

            Assert.False(MeetingHistoryService.ToDoActiveInWeek(history, originWeekStart: W2, W1));
            Assert.True(MeetingHistoryService.ToDoActiveInWeek(history, originWeekStart: W2, W2));
        }

        // ---- DistinctWeeks --------------------------------------------------

        [Fact]
        public void DistinctWeeks_ReturnsDistinctWeekStarts_NewestFirst()
        {
            var stamps = new[] { W2, W1, W3, W1, W2 };

            Assert.Equal(new[] { W3, W2, W1 }, MeetingHistoryService.DistinctWeeks(stamps));
        }

        [Fact]
        public void DistinctWeeks_IsEmpty_WhenThereIsNoData()
        {
            Assert.Empty(MeetingHistoryService.DistinctWeeks(Array.Empty<DateOnly>()));
        }
    }
}
