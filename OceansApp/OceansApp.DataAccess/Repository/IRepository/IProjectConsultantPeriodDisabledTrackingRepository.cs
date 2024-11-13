using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.ProjectsConsultantsPeriodsDisabledTrakings;


namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProjectConsultantPeriodDisabledTrackingRepository : IRepository<ProjectConsultantPeriodDisabledTracking> 
    {
        Task<List<GetProjectsConsultantsPeriodsDisabledTrakingsVM>> GetRemovedProjectsInPeriodAsync(DateTime startDate, DateTime endDate);
    }
}
