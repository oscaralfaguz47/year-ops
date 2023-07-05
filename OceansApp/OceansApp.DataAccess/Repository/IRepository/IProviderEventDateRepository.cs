
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProviderEventDateRepository : IRepository<ProviderEventDate> 
    {
        void Update(ProviderEventDate obj);
    }
}
