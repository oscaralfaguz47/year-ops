
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IApplicationUserCategoryRepository : IRepository<ApplicationUserCategory> 
    {
        void Update(ApplicationUserCategory obj);
    }
}
