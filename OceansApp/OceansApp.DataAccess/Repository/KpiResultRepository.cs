using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class KpiResultRepository : Repository<KpiResult>, IKpiResultRepository
    {
        private ApplicationDbContext _db;
        public KpiResultRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task UpsertAsync(KpiResult obj)
        {
            var existing = await GetForWeekAsync(obj.KpiDefinitionId, obj.WeekStart);
            if (existing == null)
            {
                await AddAsync(obj);
            }
            else
            {
                existing.Value = obj.Value;
                existing.Status = obj.Status;
                existing.IncludeInReview = obj.IncludeInReview;
                existing.Notes = obj.Notes;
            }
        }

        public Task<KpiResult> GetForWeekAsync(int kpiDefinitionId, DateOnly weekStart) =>
            GetFirstOrDefaultAsync(r => r.KpiDefinitionId == kpiDefinitionId && r.WeekStart == weekStart);
    }
}
