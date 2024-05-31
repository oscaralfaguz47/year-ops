
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProviderCategoryRepository : IRepository<ProviderCategory> 
    {
        void Update(ProviderCategory obj);
        Task<bool> UpdateIfExistAddIfNot(ProviderCategory obj);
    }
}
