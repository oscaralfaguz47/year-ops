
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ISystemSubAreaRepository : IRepository<SystemSubArea> 
    {
        void Update(SystemSubArea obj);
    }
}
