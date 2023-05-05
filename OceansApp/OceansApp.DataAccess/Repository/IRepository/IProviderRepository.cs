
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProviderRepository : IRepository<Provider> 
    {
        void Update(Provider obj);
        public bool UpdateIfExistAddIfNot(Provider obj);
    }
}
