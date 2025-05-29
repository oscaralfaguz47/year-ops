using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ProductsClientsCompaniesAccountingConfig;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProductClientCompanyAccountingConfigForBillingRepository : IRepository<ProductClientCompanyAccountingConfigForBilling> 
    {
        Task<MethodResponse> CreateProductClientCompanyAccountingConfigAsync(CreateUpdateProductClientCompanyAccoutingConfigVM modelData);
        Task<MethodResponse> UpdateProductClientCompanyAccountingConfigAsync(CreateUpdateProductClientCompanyAccoutingConfigVM modelData);
    }
}
