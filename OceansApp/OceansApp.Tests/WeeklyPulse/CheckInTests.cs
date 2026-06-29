using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    public class CheckInTests
    {
        private static readonly DateOnly Week = new(2026, 6, 22);
        private static readonly DateOnly NextWeek = new(2026, 6, 29);

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
        public async Task CheckIn_CanBeRecorded_AndReadBackForTeamAndWeek()
        {
            using var db = NewContext();
            var repo = new CheckInRepository(db);

            await repo.UpsertAsync(new CheckIn { TeamId = 1, WeekStart = Week, Type = CheckInType.Win, Note = "Closed a big deal" });
            await db.SaveChangesAsync();

            var saved = await repo.GetForWeekAsync(1, Week);

            Assert.NotNull(saved);
            Assert.Equal(CheckInType.Win, saved.Type);
            Assert.Equal("Closed a big deal", saved.Note);
        }

        [Fact]
        public async Task ReSaving_UpdatesSameRow_NeverDuplicates()
        {
            using var db = NewContext();
            var repo = new CheckInRepository(db);

            await repo.UpsertAsync(new CheckIn { TeamId = 1, WeekStart = Week, Type = CheckInType.Win, Note = "First" });
            await db.SaveChangesAsync();

            await repo.UpsertAsync(new CheckIn { TeamId = 1, WeekStart = Week, Type = CheckInType.Concern, Note = "Changed my mind" });
            await db.SaveChangesAsync();

            var all = await repo.GetAllAsync(c => c.TeamId == 1 && c.WeekStart == Week);
            Assert.Single(all);

            var saved = await repo.GetForWeekAsync(1, Week);
            Assert.Equal(CheckInType.Concern, saved.Type);
            Assert.Equal("Changed my mind", saved.Note);
        }

        [Fact]
        public async Task DifferentWeek_StartsBlank()
        {
            using var db = NewContext();
            var repo = new CheckInRepository(db);

            await repo.UpsertAsync(new CheckIn { TeamId = 1, WeekStart = Week, Type = CheckInType.Win, Note = "This week only" });
            await db.SaveChangesAsync();

            var nextWeek = await repo.GetForWeekAsync(1, NextWeek);

            Assert.Null(nextWeek);
        }
    }
}
