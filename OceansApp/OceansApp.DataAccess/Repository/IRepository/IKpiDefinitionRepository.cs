using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IKpiDefinitionRepository : IRepository<KpiDefinition>
    {
        void Update(KpiDefinition obj);

        Task<IEnumerable<KpiDefinition>> GetForTeamAsync(int teamId);

        /// <summary>
        /// Retires a KPI definition by clearing its <see cref="KpiDefinition.Active"/> flag.
        /// The row (and any historical results referencing it) is kept; it simply stops
        /// expecting new input and drops from Readiness and the Review.
        /// </summary>
        Task RetireAsync(int kpiDefinitionId);
    }
}
