using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    /// <summary>
    /// Repository tests for <see cref="KpiResultRepository"/>: a KPI result is a Snapshot
    /// entity with exactly one row per (KPI, Week) — re-saving upserts, never duplicates —
    /// and a new Week starts blank.
    /// </summary>
    public class KpiResultRepositoryTests
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
            db.KPI_DEFINITIONS.Add(new KpiDefinition
            {
                KpiDefinitionId = 1,
                TeamId = 1,
                Name = "On-time delivery",
                OwnerId = "leader-1",
                Target = ">= 95%",
                Active = true
            });
            db.SaveChanges();
            return db;
        }

        [Fact]
        public async Task Result_CanBeRecorded_AndReadBackForKpiAndWeek()
        {
            using var db = NewContext();
            var repo = new KpiResultRepository(db);

            await repo.UpsertAsync(new KpiResult { KpiDefinitionId = 1, WeekStart = Week, Value = "92%", Status = KpiStatus.Yellow, Notes = "Close" });
            await db.SaveChangesAsync();

            var saved = await repo.GetForWeekAsync(1, Week);

            Assert.NotNull(saved);
            Assert.Equal("92%", saved.Value);
            Assert.Equal(KpiStatus.Yellow, saved.Status);
            Assert.Equal("Close", saved.Notes);
        }

        [Fact]
        public async Task ReSaving_UpdatesSameRow_NeverDuplicates()
        {
            using var db = NewContext();
            var repo = new KpiResultRepository(db);

            await repo.UpsertAsync(new KpiResult { KpiDefinitionId = 1, WeekStart = Week, Value = "92%", Status = KpiStatus.Yellow, Notes = "First" });
            await db.SaveChangesAsync();

            await repo.UpsertAsync(new KpiResult { KpiDefinitionId = 1, WeekStart = Week, Value = "97%", Status = KpiStatus.Green, Notes = "Recovered" });
            await db.SaveChangesAsync();

            var all = await repo.GetAllAsync(r => r.KpiDefinitionId == 1 && r.WeekStart == Week);
            Assert.Single(all);

            var saved = await repo.GetForWeekAsync(1, Week);
            Assert.Equal("97%", saved.Value);
            Assert.Equal(KpiStatus.Green, saved.Status);
            Assert.Equal("Recovered", saved.Notes);
        }

        [Fact]
        public async Task IncludeInReview_RoundTrips_ThroughUpsert()
        {
            using var db = NewContext();
            var repo = new KpiResultRepository(db);

            // Recorded included by default, then re-saved un-ticked — the flag persists on update.
            await repo.UpsertAsync(new KpiResult { KpiDefinitionId = 1, WeekStart = Week, Value = "92%", Status = KpiStatus.Green, IncludeInReview = true });
            await db.SaveChangesAsync();
            Assert.True((await repo.GetForWeekAsync(1, Week)).IncludeInReview);

            await repo.UpsertAsync(new KpiResult { KpiDefinitionId = 1, WeekStart = Week, Value = "92%", Status = KpiStatus.Green, IncludeInReview = false });
            await db.SaveChangesAsync();
            Assert.False((await repo.GetForWeekAsync(1, Week)).IncludeInReview);
        }

        [Fact]
        public async Task DifferentWeek_StartsBlank()
        {
            using var db = NewContext();
            var repo = new KpiResultRepository(db);

            await repo.UpsertAsync(new KpiResult { KpiDefinitionId = 1, WeekStart = Week, Value = "92%", Status = KpiStatus.Yellow });
            await db.SaveChangesAsync();

            var nextWeek = await repo.GetForWeekAsync(1, NextWeek);

            Assert.Null(nextWeek);
        }
    }
}
