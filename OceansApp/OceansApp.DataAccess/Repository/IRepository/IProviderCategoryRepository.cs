
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProviderCategoryRepository : IRepository<ProviderCategory> 
    {
        void Update(ProviderCategory obj);
        public bool UpdateIfExistAddIfNot(ProviderCategory obj);
    }
}
