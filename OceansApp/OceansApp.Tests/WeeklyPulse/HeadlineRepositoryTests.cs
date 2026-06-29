using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Repository tests for <see cref="HeadlineRepository"/>: a Headline is a Snapshot
    /// entity scoped to (Team, Week). Unlike check-ins/KPI results there may be
    /// <b>many</b> headlines per (Team, Week) — it is the meeting's news round — so each
    /// post adds a row rather than upserting. A new Week starts blank (no carry-forward).
    /// </summary>
    public class HeadlineRepositoryTests
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
            db.TEAMS.Add(new Team { TeamId = 2, Name = "Ops", DisplayOrder = 2, TeamLeaderId = "leader-2" });
            db.SaveChanges();
            return db;
        }

        [Fact]
        public async Task Headline_CanBePosted_AndReadBackForTeamAndWeek()
        {
            using var db = NewContext();
            var repo = new HeadlineRepository(db);

            await repo.PostAsync(new Headline { TeamId = 1, WeekStart = Week, Type = HeadlineType.Highlight, Text = "Closed a big deal" });
            await db.SaveChangesAsync();

            var teamWeek = (await repo.GetForTeamWeekAsync(1, Week)).ToList();

            Assert.Single(teamWeek);
            Assert.Equal(HeadlineType.Highlight, teamWeek[0].Type);
            Assert.Equal("Closed a big deal", teamWeek[0].Text);
        }

        [Fact]
        public async Task ManyHeadlines_PerTeamWeek_AllPersist_NeverUpserted()
        {
            using var db = NewContext();
            var repo = new HeadlineRepository(db);

            await repo.PostAsync(new Headline { TeamId = 1, WeekStart = Week, Type = HeadlineType.Highlight, Text = "Win" });
            await repo.PostAsync(new Headline { TeamId = 1, WeekStart = Week, Type = HeadlineType.Risk, Text = "Concern" });
            await db.SaveChangesAsync();

            var teamWeek = (await repo.GetForTeamWeekAsync(1, Week)).ToList();

            Assert.Equal(2, teamWeek.Count);
            Assert.Contains(teamWeek, h => h.Type == HeadlineType.Highlight && h.Text == "Win");
            Assert.Contains(teamWeek, h => h.Type == HeadlineType.Risk && h.Text == "Concern");
        }

        [Fact]
        public async Task GetForWeek_ReturnsEveryTeamsHeadlines_ForThatWeek()
        {
            using var db = NewContext();
            var repo = new HeadlineRepository(db);

            await repo.PostAsync(new Headline { TeamId = 1, WeekStart = Week, Type = HeadlineType.Highlight, Text = "Sales win" });
            await repo.PostAsync(new Headline { TeamId = 2, WeekStart = Week, Type = HeadlineType.Risk, Text = "Ops risk" });
            await db.SaveChangesAsync();

            var weekAll = (await repo.GetForWeekAsync(Week)).ToList();

            Assert.Equal(2, weekAll.Count);
        }

        [Fact]
        public async Task DifferentWeek_StartsBlank()
        {
            using var db = NewContext();
            var repo = new HeadlineRepository(db);

            await repo.PostAsync(new Headline { TeamId = 1, WeekStart = Week, Type = HeadlineType.Risk, Text = "This week only" });
            await db.SaveChangesAsync();

            var nextWeek = (await repo.GetForTeamWeekAsync(1, NextWeek)).ToList();

            Assert.Empty(nextWeek);
        }
    }
}
