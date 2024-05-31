
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;
using OceansApp.Models.ViewModels.Providers;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProviderRepository : IRepository<Provider> 
    {
        Task<List<ProviderGroupByCategoryVM>> GetProvidersGroupByCategoryAsync(string providerIsActive);
        void Update(Provider obj);
        Task<int?> UpdateIfExistAddIfNot(Provider obj);
        Task<(List<ProviderGetAllWithFiltersVM> providers, int totalCount)> GetAllProviderWithFiltersAsync(ProviderGetAllForListVM filtersAndPagination);
    }
}
