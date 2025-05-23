using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Products;
namespace OceansApp.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<List<GetProductsListWithAccountingClientStatusVM>> SearchProjectsByTextWithAccountingConfigStatusAsync(string searchText, int clientId)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<GetProductsListWithAccountingClientStatusVM>();

            var client = await _db.CLIENT.FirstOrDefaultAsync(x => x.ClientId == clientId);

            if (client == null) { }

            try
            {
                var query = from p in _db.PRODUCTS
                            join acc in _db.PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG
                                on new { p.ProductId, ClientId = clientId, CompanyId = client.CompanyId } equals new { acc.ProductId, acc.ClientId, acc.CompanyId } into accGroup
                            from acc in accGroup.DefaultIfEmpty() // LEFT JOIN
                            where EF.Functions.Like(p.Name, $"%{searchText.Trim()}%")
                                  || EF.Functions.Like(p.Alias, $"%{searchText.Trim()}%")
                                  || EF.Functions.Like(p.ProductCode, $"%{searchText.Trim()}%")
                            orderby p.Name
                            select new GetProductsListWithAccountingClientStatusVM
                            {
                                ProductId = p.ProductId,
                                ProductCode = p.ProductCode,
                                ProductName = p.Name,
                                ClientHasAccountingConfig = acc != null,
                                TaxPercentage = acc != null ? acc.TaxPercentage : 0
                            };

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                // Always log or handle your exception here in real cases
                throw;
            }
        }


    }
}
