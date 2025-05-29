using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ProductsClientsCompaniesAccountingConfig;
namespace OceansApp.DataAccess.Repository
{
    public class ProductClientCompanyAccountingConfigForBillingRepository : Repository<ProductClientCompanyAccountingConfigForBilling>, IProductClientCompanyAccountingConfigForBillingRepository
    {
        private ApplicationDbContext _db;
        public ProductClientCompanyAccountingConfigForBillingRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<MethodResponse> CreateProductClientCompanyAccountingConfigAsync(CreateUpdateProductClientCompanyAccoutingConfigVM modelData)
        {
            var client = await _db.CLIENT.FirstOrDefaultAsync(x => x.ClientId == modelData.ClientId);

            if (client == null) return MethodResponse
                   .CreateFailureExceptionResponse($"The Client was not found in de database.");

            var existsWithSamePrimaryKey = await _db.PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG.FirstOrDefaultAsync(x => x.ProductId == modelData.ProductId && x.ClientId == modelData.ClientId 
            && x.CompanyId == client.CompanyId);

            if (existsWithSamePrimaryKey != null) return MethodResponse
                    .CreateFailureValidationResponse($"There is already a configuration with the same Product, Client and Company.");


            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    ProductClientCompanyAccountingConfigForBilling modelToCreate = new()
                    {
                        ProductId = (int)modelData.ProductId,
                        ClientId = (int)modelData.ClientId,
                        CompanyId = client.CompanyId,
                        MovementTypeId = modelData.MovementTypeId,
                        CostCenterIdSales = (int)modelData.CostCenterIdSales,
                        CostCenterIdSalesDiscount = (int)modelData.CostCenterIdSalesDiscount,
                        CostCenterIdSalesReturn = (int)modelData.CostCenterIdSalesReturn,
                        CostCenterIdTaxPercentage = modelData.CostCenterIdSalesTax,
                        AccountingAccountIdSales = (int)modelData.AccountingAccountIdSales,
                        AccountingAccountIdSalesDiscount = (int)modelData.AccountingAccountIdSalesDiscount,
                        AccountingAccountIdSalesReturn = (int)modelData.AccountingAccountIdSalesReturn,
                        AccountingAccountIdTaxPercentage = modelData.AccountingAccountIdSalesTax,
                        TaxPercentage = (decimal)modelData.TaxPercentage
                    };

                    var createdAccountingConfig = await _db.PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG.AddAsync(modelToCreate);

                    await _db.SaveChangesAsync();
                    if (createdAccountingConfig.Entity != null)
                    {
                        await transaction.CommitAsync();

                        return MethodResponse.CreateSuccessResponse("The Accounting Configuration was created successfully!");
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"Something went wrong creating the Accounting Config, please try again." };
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
                }
            }
        }

        public async Task<MethodResponse> UpdateProductClientCompanyAccountingConfigAsync(CreateUpdateProductClientCompanyAccoutingConfigVM modelData)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var client = await _db.CLIENT.FirstOrDefaultAsync(x => x.ClientId == modelData.ClientId);

                    if (client == null) return MethodResponse
                           .CreateFailureExceptionResponse($"The Client was not found in de database.");

                    var existingAccountingConfig = await _db.PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG.FirstOrDefaultAsync(x => x.ProductId == modelData.ProductId && x.ClientId == modelData.ClientId
                    && x.CompanyId == client.CompanyId);
                    if (existingAccountingConfig == null)
                    {
                        await transaction.RollbackAsync();
                        return new MethodResponse { MessageType = "Not Found", Success = false, Message = $"The AccountingConfig was not found." };
                    }

                    existingAccountingConfig.CostCenterIdSales = (int)modelData.CostCenterIdSales;
                    existingAccountingConfig.CostCenterIdSalesDiscount = (int)modelData.CostCenterIdSalesDiscount;
                    existingAccountingConfig.CostCenterIdSalesReturn = (int)modelData.CostCenterIdSalesReturn;
                    existingAccountingConfig.CostCenterIdTaxPercentage = modelData.CostCenterIdSalesTax;

                    existingAccountingConfig.AccountingAccountIdSales = (int)modelData.AccountingAccountIdSales;
                    existingAccountingConfig.AccountingAccountIdSalesDiscount = (int)modelData.AccountingAccountIdSalesDiscount;
                    existingAccountingConfig.AccountingAccountIdSalesReturn = (int)modelData.AccountingAccountIdSalesReturn;
                    existingAccountingConfig.AccountingAccountIdTaxPercentage = modelData.AccountingAccountIdSalesTax;

                    existingAccountingConfig.TaxPercentage = (decimal)modelData.TaxPercentage;

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new MethodResponse { Success = true, Message = $"The Accounting Configuration was updated successfully." };
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
