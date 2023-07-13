
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;
using OceansApp.Models.ViewModels.Providers;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProviderRepository : IRepository<Provider> 
    {
        Task<List<ProviderGroupByCategoryVM>> GetProvidersGroupByCategoryAsync(string providerIsActive);
        void Update(Provider obj);
        public int? UpdateIfExistAddIfNot(Provider obj);
        Task<List<ProviderGetAllWithFiltersVM>> GetAllProviderWithFiltersAsync(ProviderFiltersGetAllVM filtersAndPagination);
    }
}
