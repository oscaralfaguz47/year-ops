using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class IssueRepository : Repository<Issue>, IIssueRepository
    {
        private ApplicationDbContext _db;
        public IssueRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task RaiseAsync(Issue issue, DateTimeOffset at)
        {
            issue.History.Add(new IssueHistory
            {
                WeekStart = issue.OriginWeekStart,
                ChangeType = IssueChangeType.Status,
                Status = IssueStatus.Open,
                CreatedAt = at
            });
            await AddAsync(issue);
        }

        public async Task TransitionAsync(int issueId, IssueStatus status, DateOnly weekStart, DateTimeOffset at)
        {
            await _db.ISSUE_HISTORIES.AddAsync(new IssueHistory
            {
                IssueId = issueId,
                WeekStart = weekStart,
                ChangeType = IssueChangeType.Status,
                Status = status,
                CreatedAt = at
            });
        }

        public async Task CommentAsync(int issueId, string comment, DateOnly weekStart, DateTimeOffset at)
        {
            await _db.ISSUE_HISTORIES.AddAsync(new IssueHistory
            {
                IssueId = issueId,
                WeekStart = weekStart,
                ChangeType = IssueChangeType.Comment,
                Comment = comment,
                CreatedAt = at
            });
        }

        public async Task<IEnumerable<Issue>> GetForTeamAsync(int teamId) =>
            await GetAllAsync(filter: i => i.TeamId == teamId, includeProperties: nameof(Issue.History));
    }
}
