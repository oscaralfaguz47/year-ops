using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProjectConsultantAssignedHistoryActionRepository : IRepository<ProjectConsultantAssignedHistoryAction> 
    {
        void Update(ProjectConsultantAssignedHistoryAction obj);

    }
}
