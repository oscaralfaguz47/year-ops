using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
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

        public async Task<MethodResponse> CreateProductAsync(CreateUpdateProductVM productData)
        {
            var existsProductWithSameName = await _db.PRODUCTS.FirstOrDefaultAsync(x => x.Name == productData.ProductName.Trim());

            if (existsProductWithSameName != null) return MethodResponse
                    .CreateFailureValidationResponse($"The Product Name: '{productData.ProductName}' already exists in the database, try with another.");

            var productConsecutive = await _db.GLOBAL_CONSECUTIVES.FirstOrDefaultAsync(x => x.Name == "PRODUCTS");

            if (productConsecutive == null) return MethodResponse
                    .CreateFailureNotFoundResponse($"The PRODUCTS global consecutive was not found");

            int currentConsecutiveNumber = productConsecutive.ConsecutiveNumber;
            currentConsecutiveNumber++;

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    Product productToCreate = new()
                    {
                        Name = productData.ProductName.Trim(),
                        Alias = productData.Alias.Trim(),
                        ProductCode = $"OCE_{currentConsecutiveNumber.ToString("D6")}",
                        Detail = productData.Detail
                    };

                    var createdProduct = await _db.PRODUCTS.AddAsync(productToCreate);

                    await _db.SaveChangesAsync();
                    if (createdProduct.Entity.ProductId > 0)
                    {
                        productConsecutive.ConsecutiveNumber++;
                        await _db.SaveChangesAsync();
                        await transaction.CommitAsync();
                        GetProductsListWithAccountingClientStatusVM genericObjectToReturn = new()
                        {
                            ProductId = createdProduct.Entity.ProductId,
                            ProductCode = createdProduct.Entity.ProductCode,
                            ProductName = createdProduct.Entity.Name,
                            TaxPercentage = 0,
                            ClientHasAccountingConfig = false
                        };
                        return new MethodResponse
                        {
                            Success = true,
                            Message = $"The Product '{productData.ProductName.Trim()}' was created successfully.",
                            GenericObject = genericObjectToReturn
                        };
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"Something went wrong creating the product, please try again." };
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
                }
            }
        }

        public async Task<MethodResponse> UpdateProductAsync(CreateUpdateProductVM productData)
        {
            var existsProductWithSameName = await _db.PRODUCTS.FirstOrDefaultAsync(x => x.Name == productData.ProductName.Trim() && x.ProductId == productData.ProductId);

            if (existsProductWithSameName != null) return MethodResponse
                    .CreateFailureValidationResponse($"The Product Name: '{productData.ProductName}' already exists in the database, try with another.");

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingProduct = await _db.PRODUCTS.FirstOrDefaultAsync(x => x.ProductId == productData.ProductId);
                    if (existingProduct == null)
                    {
                        await transaction.RollbackAsync();
                        return new MethodResponse { MessageType = "Not Found", Success = false, Message = $"The Product was not found." };
                    }

                    existingProduct.Name = productData.ProductName.Trim();
                    existingProduct.Alias = productData.Alias.Trim();
                    existingProduct.Detail = productData.Detail.Trim();

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new MethodResponse { Success = true, Message = $"The Product was updated successfully." };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
                }
            }
        }


    }
}
