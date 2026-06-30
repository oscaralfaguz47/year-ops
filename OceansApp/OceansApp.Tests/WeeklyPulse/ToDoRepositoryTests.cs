using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository;
using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.Tests.WeeklyPulse
{
    public class ToDoRepositoryTests
    {
        private static readonly DateOnly W1 = new(2026, 6, 1);
        private static readonly DateOnly W2 = new(2026, 6, 8);
        private static readonly DateOnly W3 = new(2026, 6, 15);
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

        private static ToDo NewToDo() => new()
        {
            TeamId = 1,
            Title = "Send the renewal quote",
            OwnerId = "owner-1",
            DueDate = Due,
            OriginWeekStart = W1
        };

        [Fact]
        public async Task RaiseAsync_CreatesToDo_WithSingleOpenStatusRow()
        {
            using var db = NewContext();
            var repo = new ToDoRepository(db);

            await repo.RaiseAsync(NewToDo(), At(W1));
            await db.SaveChangesAsync();

            var toDo = Assert.Single(await repo.GetForTeamAsync(1));
            Assert.Equal(W1, toDo.OriginWeekStart);
            Assert.Equal("owner-1", toDo.OwnerId);
            Assert.Equal(Due, toDo.DueDate);
            var row = Assert.Single(toDo.History);
            Assert.Equal(ToDoChangeType.Status, row.ChangeType);
            Assert.Equal(ToDoStatus.Open, row.Status);
            Assert.Equal(W1, row.WeekStart);
            Assert.Equal(ToDoStatus.Open, ToDoStateService.StateAsOf(toDo.History, W1));
        }

        [Fact]
        public async Task Transitions_AppendHistoryRows_AndStateAsOfTracksThem()
        {
            using var db = NewContext();
            var repo = new ToDoRepository(db);

            await repo.RaiseAsync(NewToDo(), At(W1));
            await db.SaveChangesAsync();

            var toDoId = (await repo.GetForTeamAsync(1)).Single().ToDoId;

            await repo.TransitionAsync(toDoId, ToDoStatus.Blocked, W2, At(W2));
            await db.SaveChangesAsync();
            await repo.TransitionAsync(toDoId, ToDoStatus.Done, W3, At(W3));
            await db.SaveChangesAsync();

            var toDo = (await repo.GetForTeamAsync(1)).Single();

            // One row per change: Open + Blocked + Done.
            Assert.Equal(3, toDo.History.Count(h => h.ChangeType == ToDoChangeType.Status));

            Assert.Equal(ToDoStatus.Open, ToDoStateService.StateAsOf(toDo.History, W1));
            Assert.Equal(ToDoStatus.Blocked, ToDoStateService.StateAsOf(toDo.History, W2));
            Assert.Equal(ToDoStatus.Done, ToDoStateService.StateAsOf(toDo.History, W3));
        }

        [Fact]
        public async Task CommentAsync_AppendsCommentRow_WithoutChangingState()
        {
            using var db = NewContext();
            var repo = new ToDoRepository(db);

            await repo.RaiseAsync(NewToDo(), At(W1));
            await db.SaveChangesAsync();

            var toDoId = (await repo.GetForTeamAsync(1)).Single().ToDoId;

            await repo.CommentAsync(toDoId, "Waiting on legal sign-off", W2, At(W2));
            await db.SaveChangesAsync();

            var toDo = (await repo.GetForTeamAsync(1)).Single();
            var comment = Assert.Single(toDo.History, h => h.ChangeType == ToDoChangeType.Comment);
            Assert.Equal("Waiting on legal sign-off", comment.Comment);
            Assert.Equal(W2, comment.WeekStart);
            Assert.Equal(ToDoStatus.Open, ToDoStateService.StateAsOf(toDo.History, W2));
        }
    }
}
