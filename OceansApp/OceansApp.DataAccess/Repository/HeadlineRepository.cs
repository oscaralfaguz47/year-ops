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

        public Task<IEnumerable<Headline>> GetForWeekAsync(DateOnly weekStart) =>
            GetAllAsync(h => h.WeekStart == weekStart, orderBy: q => q.OrderBy(h => h.HeadlineId));

        public Task<IEnumerable<Headline>> GetForTeamWeekAsync(int teamId, DateOnly weekStart) =>
            GetAllAsync(h => h.TeamId == teamId && h.WeekStart == weekStart, orderBy: q => q.OrderBy(h => h.HeadlineId));
    }
}
