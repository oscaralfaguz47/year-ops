using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Projects;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProjectConsultantAssignedHistoryRepository : IRepository<ProjectConsultantAssignedHistory> 
    {
        Task<List<GetProjectConsultantAssignedHistoryVM>> GetProjectConsultantAssignedHistoryByAssignationId(int projectConsultantAssignedId);
        void Update(ProjectConsultantAssignedHistory obj);

    }
}
