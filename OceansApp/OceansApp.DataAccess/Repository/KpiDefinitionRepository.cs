using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class KpiDefinitionRepository : Repository<KpiDefinition>, IKpiDefinitionRepository
    {
        private ApplicationDbContext _db;
        public KpiDefinitionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(KpiDefinition obj)
        {
            _db.KPI_DEFINITIONS.Update(obj);
        }

        public async Task<IEnumerable<KpiDefinition>> GetForTeamAsync(int teamId) =>
            await GetAllAsync(filter: k => k.TeamId == teamId, orderBy: q => q.OrderBy(k => k.Name));

        public async Task RetireAsync(int kpiDefinitionId)
        {
            var kpi = await _db.KPI_DEFINITIONS
                .FirstOrDefaultAsync(k => k.KpiDefinitionId == kpiDefinitionId)
                ?? throw new InvalidOperationException($"No KPI definition {kpiDefinitionId}.");

            // Retire = stop expecting input; the row and its history are kept untouched.
            kpi.Active = false;
        }
    }
}
