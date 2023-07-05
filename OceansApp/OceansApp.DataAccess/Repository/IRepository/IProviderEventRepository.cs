
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProviderEventRepository : IRepository<ProviderEvent> 
    {
        void Update(ProviderEvent obj);
    }
}
