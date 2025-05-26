using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Products;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product> 
    {
        Task<List<GetProductsListWithAccountingClientStatusVM>> SearchProductsByTextWithAccountingConfigStatusAsync(string searchText, int clientId);
        Task<MethodResponse> CreateProductAsync(CreateUpdateProductVM projectData);
        Task<MethodResponse> UpdateProductAsync(CreateUpdateProductVM productData);
    }
}
