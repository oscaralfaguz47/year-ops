using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository;
using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    public class KpiDefinitionRepositoryTests
    {
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

        private static KpiDefinition NewKpi(int teamId = 1) => new()
        {
            TeamId = teamId,
            Name = "On-time delivery",
            OwnerId = "owner-1",
            Target = ">= 95%"
        };

        [Fact]
        public async Task AddAsync_CreatesDefinition_DefaultingToLiveAndInScope()
        {
            using var db = NewContext();
            var repo = new KpiDefinitionRepository(db);

            await repo.AddAsync(NewKpi());
            await db.SaveChangesAsync();

            var kpi = Assert.Single(await repo.GetForTeamAsync(1));
            Assert.Equal("On-time delivery", kpi.Name);
            Assert.Equal(">= 95%", kpi.Target);
            Assert.True(kpi.Active);
            Assert.True(kpi.InScope);
        }

        [Fact]
        public async Task GetForTeamAsync_OnlyReturnsThatTeamsDefinitions()
        {
            using var db = NewContext();
            var repo = new KpiDefinitionRepository(db);

            await repo.AddAsync(NewKpi(teamId: 1));
            await repo.AddAsync(NewKpi(teamId: 2));
            await db.SaveChangesAsync();

            Assert.Single(await repo.GetForTeamAsync(1));
            Assert.Single(await repo.GetForTeamAsync(2));
        }

        [Fact]
        public async Task Update_EditsFlagsIndependently_WithoutTouchingTheOther()
        {
            using var db = NewContext();
            var repo = new KpiDefinitionRepository(db);

            await repo.AddAsync(NewKpi());
            await db.SaveChangesAsync();

            var kpi = (await repo.GetForTeamAsync(1)).Single();
            kpi.InScope = false; // out of meeting scope, but still live
            repo.Update(kpi);
            await db.SaveChangesAsync();

            var saved = (await repo.GetForTeamAsync(1)).Single();
            Assert.True(saved.Active);
            Assert.False(saved.InScope);
        }

        [Fact]
        public async Task RetireAsync_KeepsTheRow_ButStopsItExpectingInput()
        {
            using var db = NewContext();
            var repo = new KpiDefinitionRepository(db);

            await repo.AddAsync(NewKpi());
            await db.SaveChangesAsync();
            var id = (await repo.GetForTeamAsync(1)).Single().KpiDefinitionId;

            await repo.RetireAsync(id);
            await db.SaveChangesAsync();

            // The definition (and any historical results referencing it) survives — only Active flips.
            var kpi = (await repo.GetForTeamAsync(1)).Single();
            Assert.False(kpi.Active);
            Assert.False(KpiScopeService.ExpectsInput(kpi));

            // Retiring does not silently touch the scope flag — the two are independent.
            Assert.True(kpi.InScope);
        }
    }
}
