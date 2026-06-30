using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Domain.WeeklyPulse;
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

        public async Task<Issue> ConvertCheckInAsync(int checkInId, DateOnly weekStart, DateTimeOffset at)
        {
            // Additive conversion: read the source, never mutate or remove it.
            var checkIn = await _db.CHECK_INS.FirstOrDefaultAsync(c => c.CheckInId == checkInId)
                ?? throw new InvalidOperationException($"No check-in {checkInId}.");

            var issue = ConversionService.FromCheckIn(checkIn, weekStart);
            await RaiseAsync(issue, at);
            return issue;
        }

        public async Task<Issue> ConvertHeadlineAsync(int headlineId, DateOnly weekStart, DateTimeOffset at)
        {
            // Additive conversion: the headline stays intact in its Week.
            var headline = await _db.HEADLINES.FirstOrDefaultAsync(h => h.HeadlineId == headlineId)
                ?? throw new InvalidOperationException($"No headline {headlineId}.");

            var issue = ConversionService.FromHeadline(headline, weekStart);
            await RaiseAsync(issue, at);
            return issue;
        }

        public async Task SetPinAsync(int issueId, bool pinned, DateOnly weekStart)
        {
            var issue = await _db.ISSUES
                .Include(i => i.History)
                .FirstOrDefaultAsync(i => i.IssueId == issueId)
                ?? throw new InvalidOperationException($"No issue {issueId}.");

            // Pin is a Deferred-only override — rejected at the model level otherwise.
            ReviewSurfacingService.EnsurePinnable(IssueStateService.StateAsOf(issue.History, weekStart));

            issue.Pinned = pinned;
        }

        public async Task DeleteHistoryForWeekAsync(DateOnly weekStart)
        {
            var rows = await _db.ISSUE_HISTORIES
                .Where(h => h.WeekStart == weekStart)
                .ToListAsync();
            _db.ISSUE_HISTORIES.RemoveRange(rows);
        }
    }
}
