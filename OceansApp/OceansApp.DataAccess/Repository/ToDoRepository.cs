using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Domain.WeeklyPulse;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ToDoRepository : Repository<ToDo>, IToDoRepository
    {
        private ApplicationDbContext _db;
        public ToDoRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task RaiseAsync(ToDo toDo, DateTimeOffset at)
        {
            toDo.History.Add(new ToDoHistory
            {
                WeekStart = toDo.OriginWeekStart,
                ChangeType = ToDoChangeType.Status,
                Status = ToDoStatus.Open,
                CreatedAt = at
            });
            await AddAsync(toDo);
        }

        public async Task TransitionAsync(int toDoId, ToDoStatus status, DateOnly weekStart, DateTimeOffset at)
        {
            await _db.TODO_HISTORIES.AddAsync(new ToDoHistory
            {
                ToDoId = toDoId,
                WeekStart = weekStart,
                ChangeType = ToDoChangeType.Status,
                Status = status,
                CreatedAt = at
            });
        }

        public async Task CommentAsync(int toDoId, string comment, DateOnly weekStart, DateTimeOffset at)
        {
            await _db.TODO_HISTORIES.AddAsync(new ToDoHistory
            {
                ToDoId = toDoId,
                WeekStart = weekStart,
                ChangeType = ToDoChangeType.Comment,
                Comment = comment,
                CreatedAt = at
            });
        }

        public async Task<IEnumerable<ToDo>> GetForTeamAsync(int teamId) =>
            await GetAllAsync(filter: t => t.TeamId == teamId, includeProperties: nameof(ToDo.History));

        public async Task<ToDo> ConvertIssueAsync(int issueId, string ownerId, DateOnly dueDate, DateOnly weekStart, DateTimeOffset at)
        {
            // Additive conversion: read the source Issue, never mutate or remove it.
            var issue = await _db.ISSUES.FirstOrDefaultAsync(i => i.IssueId == issueId)
                ?? throw new InvalidOperationException($"No issue {issueId}.");

            var toDo = ConversionService.FromIssue(issue, ownerId, dueDate, weekStart);
            await RaiseAsync(toDo, at);
            return toDo;
        }
    }
}
