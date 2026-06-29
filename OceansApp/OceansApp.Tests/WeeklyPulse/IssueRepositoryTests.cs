using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository;
using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    public class IssueRepositoryTests
    {
        private static readonly DateOnly W1 = new(2026, 6, 1);
        private static readonly DateOnly W2 = new(2026, 6, 8);
        private static readonly DateOnly W3 = new(2026, 6, 15);

        private static DateTimeOffset At(DateOnly week, int seq = 0) =>
            new DateTimeOffset(week.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).AddMinutes(seq);

        private static ApplicationDbContext NewContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var db = new ApplicationDbContext(options);

            db.TEAMS.Add(new Team { TeamId = 1, Name = "Sales", DisplayOrder = 1, TeamLeaderId = "leader-1" });
            db.SaveChanges();
            return db;
        }

        [Fact]
        public async Task RaiseAsync_CreatesIssue_WithSingleOpenStatusRow()
        {
            using var db = NewContext();
            var repo = new IssueRepository(db);

            await repo.RaiseAsync(new Issue
            {
                TeamId = 1,
                Title = "Pricing page confuses enterprise leads",
                Priority = IssuePriority.High,
                OriginWeekStart = W1
            }, At(W1));
            await db.SaveChangesAsync();

            var issues = (await repo.GetForTeamAsync(1)).ToList();
            var issue = Assert.Single(issues);

            Assert.Equal(W1, issue.OriginWeekStart);
            Assert.Equal(IssuePriority.High, issue.Priority);
            var row = Assert.Single(issue.History);
            Assert.Equal(IssueChangeType.Status, row.ChangeType);
            Assert.Equal(IssueStatus.Open, row.Status);
            Assert.Equal(W1, row.WeekStart);
            Assert.Equal(IssueStatus.Open, IssueStateService.StateAsOf(issue.History, W1));
        }

        [Fact]
        public async Task Transitions_AppendHistoryRows_AndStateAsOfTracksThem()
        {
            using var db = NewContext();
            var repo = new IssueRepository(db);

            await repo.RaiseAsync(new Issue
            {
                TeamId = 1,
                Title = "Office move logistics",
                Priority = IssuePriority.Med,
                OriginWeekStart = W1
            }, At(W1));
            await db.SaveChangesAsync();

            var issueId = (await repo.GetForTeamAsync(1)).Single().IssueId;

            await repo.TransitionAsync(issueId, IssueStatus.Deferred, W2, At(W2));
            await db.SaveChangesAsync();
            await repo.TransitionAsync(issueId, IssueStatus.Solved, W3, At(W3));
            await db.SaveChangesAsync();

            var issue = (await repo.GetForTeamAsync(1)).Single();

            // One row per change: Open + Deferred + Solved.
            Assert.Equal(3, issue.History.Count(h => h.ChangeType == IssueChangeType.Status));

            // stateAsOf returns the correct state for any past week.
            Assert.Equal(IssueStatus.Open, IssueStateService.StateAsOf(issue.History, W1));
            Assert.Equal(IssueStatus.Deferred, IssueStateService.StateAsOf(issue.History, W2));
            Assert.Equal(IssueStatus.Solved, IssueStateService.StateAsOf(issue.History, W3));
        }

        [Fact]
        public async Task CommentAsync_AppendsCommentRow_WithoutChangingState()
        {
            using var db = NewContext();
            var repo = new IssueRepository(db);

            await repo.RaiseAsync(new Issue
            {
                TeamId = 1,
                Title = "Rename the newsletter",
                Priority = IssuePriority.Low,
                OriginWeekStart = W1
            }, At(W1));
            await db.SaveChangesAsync();

            var issueId = (await repo.GetForTeamAsync(1)).Single().IssueId;

            await repo.CommentAsync(issueId, "Identified: name no longer fits the audience", W2, At(W2));
            await db.SaveChangesAsync();

            var issue = (await repo.GetForTeamAsync(1)).Single();
            var comment = Assert.Single(issue.History, h => h.ChangeType == IssueChangeType.Comment);
            Assert.Equal("Identified: name no longer fits the audience", comment.Comment);
            Assert.Equal(W2, comment.WeekStart);
            Assert.Equal(IssueStatus.Open, IssueStateService.StateAsOf(issue.History, W2));
        }

        [Fact]
        public async Task SetPinAsync_PinsDeferredIssue()
        {
            using var db = NewContext();
            var repo = new IssueRepository(db);

            await repo.RaiseAsync(new Issue
            {
                TeamId = 1,
                Title = "Vendor SLA review",
                Priority = IssuePriority.Med,
                OriginWeekStart = W1
            }, At(W1));
            await db.SaveChangesAsync();

            var issueId = (await repo.GetForTeamAsync(1)).Single().IssueId;
            await repo.TransitionAsync(issueId, IssueStatus.Deferred, W2, At(W2));
            await db.SaveChangesAsync();

            await repo.SetPinAsync(issueId, pinned: true, W2);
            await db.SaveChangesAsync();

            Assert.True((await repo.GetForTeamAsync(1)).Single().Pinned);
        }

        [Fact]
        public async Task SetPinAsync_RejectsNonDeferredIssue()
        {
            using var db = NewContext();
            var repo = new IssueRepository(db);

            // Issue is Open (never Deferred) — pinning must be rejected at the model level.
            await repo.RaiseAsync(new Issue
            {
                TeamId = 1,
                Title = "Open issue",
                Priority = IssuePriority.Med,
                OriginWeekStart = W1
            }, At(W1));
            await db.SaveChangesAsync();

            var issueId = (await repo.GetForTeamAsync(1)).Single().IssueId;

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.SetPinAsync(issueId, pinned: true, W1));
        }
    }
}
