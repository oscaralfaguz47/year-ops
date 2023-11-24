
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ISystemAreaRepository : IRepository<SystemArea> 
    {
        void Update(SystemArea obj);
    }
}
