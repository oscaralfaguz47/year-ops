using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    public class TeamLeadershipTests
    {
        private static ApplicationDbContext NewContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task OnePerson_CanLead_MoreThanOneTeam()
        {
            using var db = NewContext();
            var repo = new TeamRepository(db);

            // The same leader heads two teams — Settings must allow this.
            await repo.AddAsync(new Team { Name = "Sales", DisplayOrder = 1, TeamLeaderId = "leader-1" });
            await repo.AddAsync(new Team { Name = "Marketing", DisplayOrder = 2, TeamLeaderId = "leader-1" });
            await db.SaveChangesAsync();

            var led = await repo.GetAllAsync(filter: t => t.TeamLeaderId == "leader-1");
            Assert.Equal(2, led.Count());
        }
    }
}
