
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser> 
    {
        void Update(ApplicationUser obj);
    }
}
