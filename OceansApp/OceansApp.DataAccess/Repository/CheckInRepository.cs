using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class CheckInRepository : Repository<CheckIn>, ICheckInRepository
    {
        private ApplicationDbContext _db;
        public CheckInRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(CheckIn obj)
        {
            _db.CHECK_INS.Update(obj);
        }

        public async Task UpsertAsync(CheckIn obj)
        {
            var existing = await GetForWeekAsync(obj.TeamId, obj.WeekStart);
            if (existing == null)
            {
                await AddAsync(obj);
            }
            else
            {
                existing.Type = obj.Type;
                existing.Note = obj.Note;
            }
        }

        public Task<CheckIn> GetForWeekAsync(int teamId, DateOnly weekStart) =>
            GetFirstOrDefaultAsync(c => c.TeamId == teamId && c.WeekStart == weekStart);
    }
}
