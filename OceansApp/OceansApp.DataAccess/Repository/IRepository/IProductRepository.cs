using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Products;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product> 
    {
        Task<List<GetProductsListWithAccountingClientStatusVM>> SearchProjectsByTextWithAccountingConfigStatusAsync(string searchText, int clientId);
    }
}
