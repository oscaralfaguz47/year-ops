using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class HeadlineRepository : Repository<Headline>, IHeadlineRepository
    {
        private ApplicationDbContext _db;
        public HeadlineRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public Task PostAsync(Headline obj) => AddAsync(obj);

        public async Task EditAsync(int headlineId, int teamId, HeadlineType type, string text)
        {
            var headline = await _db.HEADLINES.FirstOrDefaultAsync(h => h.HeadlineId == headlineId)
                ?? throw new InvalidOperationException($"No headline {headlineId}.");

            // A headline is a Snapshot: it stays in its Week; only its Team, type and text move.
            headline.TeamId = teamId;
            headline.Type = type;
            headline.Text = text;
        }

        public Task<IEnumerable<Headline>> GetForWeekAsync(DateOnly weekStart) =>
            GetAllAsync(h => h.WeekStart == weekStart, orderBy: q => q.OrderBy(h => h.HeadlineId));

        public Task<IEnumerable<Headline>> GetForTeamWeekAsync(int teamId, DateOnly weekStart) =>
            GetAllAsync(h => h.TeamId == teamId && h.WeekStart == weekStart, orderBy: q => q.OrderBy(h => h.HeadlineId));
    }
}
