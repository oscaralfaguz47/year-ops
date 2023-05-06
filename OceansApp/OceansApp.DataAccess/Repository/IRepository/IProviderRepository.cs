
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProviderRepository : IRepository<Provider> 
    {
        Task<List<ProviderGroupByCategoryVM>> GetProvidersGroupByCategoryAsync(string providerIsActive);
        void Update(Provider obj);
        public bool UpdateIfExistAddIfNot(Provider obj);
    }
}
