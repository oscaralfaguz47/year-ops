using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository;
using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Conversions end-to-end at the repository layer (WP-2.6): each conversion persists a
    /// new pre-filled Living entity with its initial Open status row and an origin
    /// back-reference, while the source record stays intact in its Week — never consumed.
    /// </summary>
    public class ConversionRepositoryTests
    {
        private static readonly DateOnly SourceWeek = new(2026, 6, 1);
        private static readonly DateOnly ConversionWeek = new(2026, 6, 15);
        private static readonly DateOnly Due = new(2026, 6, 30);

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
        public async Task ConvertHeadlineAsync_CreatesPrefilledIssue_AndLeavesHeadlineIntact()
        {
            using var db = NewContext();
            db.HEADLINES.Add(new Headline
            {
                TeamId = 1,
                WeekStart = SourceWeek,
                Type = HeadlineType.Risk,
                Text = "Client churn risk"
            });
            await db.SaveChangesAsync();
            var headlineId = db.HEADLINES.Single().HeadlineId;

            var issueRepo = new IssueRepository(db);
            await issueRepo.ConvertHeadlineAsync(headlineId, ConversionWeek, At(ConversionWeek));
            await db.SaveChangesAsync();

            var stored = Assert.Single(await issueRepo.GetForTeamAsync(1));
            Assert.Equal("[from headline] Client churn risk", stored.Title);
            Assert.Equal(IssuePriority.High, stored.Priority);
            Assert.Equal(OriginType.Headline, stored.OriginType);
            Assert.Equal(headlineId, stored.OriginId);
            Assert.Equal(IssueStatus.Open, IssueStateService.StateAsOf(stored.History, ConversionWeek));

            // Source headline is preserved in its Week.
            var source = Assert.Single(db.HEADLINES);
            Assert.Equal(headlineId, source.HeadlineId);
            Assert.Equal("Client churn risk", source.Text);
            Assert.Equal(SourceWeek, source.WeekStart);
        }

        [Fact]
        public async Task ConvertIssueAsync_CreatesPrefilledToDo_AndLeavesIssueIntact()
        {
            using var db = NewContext();
            var issueRepo = new IssueRepository(db);
            await issueRepo.RaiseAsync(new Issue
            {
                TeamId = 1,
                Title = "Decide pricing",
                Priority = IssuePriority.High,
                OriginWeekStart = SourceWeek
            }, At(SourceWeek));
            await db.SaveChangesAsync();
            var issueId = (await issueRepo.GetForTeamAsync(1)).Single().IssueId;

            var toDoRepo = new ToDoRepository(db);
            await toDoRepo.ConvertIssueAsync(issueId, "owner-9", Due, ConversionWeek, At(ConversionWeek));
            await db.SaveChangesAsync();

            var stored = Assert.Single(await toDoRepo.GetForTeamAsync(1));
            Assert.Equal("[from issue] Decide pricing", stored.Title);
            Assert.Equal("owner-9", stored.OwnerId);
            Assert.Equal(Due, stored.DueDate);
            Assert.Equal(OriginType.Issue, stored.OriginType);
            Assert.Equal(issueId, stored.OriginId);
            Assert.Equal(ToDoStatus.Open, ToDoStateService.StateAsOf(stored.History, ConversionWeek));

            // Source Issue is preserved, with its own history untouched.
            var source = Assert.Single(await issueRepo.GetForTeamAsync(1));
            Assert.Equal(issueId, source.IssueId);
            Assert.Equal("Decide pricing", source.Title);
            Assert.Equal(IssueStatus.Open, IssueStateService.StateAsOf(source.History, ConversionWeek));
        }

        [Fact]
        public async Task ConvertHeadlineAsync_Throws_WhenSourceMissing()
        {
            using var db = NewContext();
            var issueRepo = new IssueRepository(db);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => issueRepo.ConvertHeadlineAsync(999, ConversionWeek, At(ConversionWeek)));
        }
    }
}
