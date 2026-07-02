using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.ProjectConsultantAssigned;
using OceansApp.Models.ViewModels.Projects;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProjectConsultantAssignedRepository : IRepository<ProjectConsultantAssigned> 
    {
        Task<List<GetProjectsListVM>> GetProjectsWhereConsultantAssigned(string? userId);
        Task<GetConsultantSelectedProjectInfoVM> GetConsultantSelectedProjectInfo(string userId);
        Task<GetConsultantStatusInTheProjectVM> GetConsultantStatusInTheProject(string userId, DateTime startDate,
            DateTime endDate);
        Task<List<ProjectActiveAndAccessTrackingTool>> GetProjectsWhereActiveAndAccessToTrackingTool(int consultantId);
        Task<List<ProjectAndSuccesManagerInfoVM>> GetProjectsAndSuccessManagersWhereConsultantIsActiveAsync(int consultantId);
    }
}
