using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IKpiResultRepository : IRepository<KpiResult>
    {
        /// <summary>
        /// Inserts the result for its (KpiDefinitionId, WeekStart), or overwrites the existing
        /// row's <see cref="KpiResult.Value"/>, <see cref="KpiResult.Status"/> and
        /// <see cref="KpiResult.Notes"/> if one already exists — guaranteeing exactly one
        /// result per (KPI, Week). Does not save; the caller commits via
        /// <see cref="IUnitOfWork.SaveAsync"/>.
        /// </summary>
        Task UpsertAsync(KpiResult obj);

        /// <summary>
        /// Returns the result for the given (KPI, Week), or null when the Week is blank.
        /// </summary>
        Task<KpiResult> GetForWeekAsync(int kpiDefinitionId, DateOnly weekStart);
    }
}
