using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProjectRepository : IRepository<Project> 
    {
        void Update(Project obj);

    }
}
