using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProjectConsultantAssignedRepository : IRepository<ProjectConsultantAssigned> 
    {
        void Update(ProjectConsultantAssigned obj);

    }
}
