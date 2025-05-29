using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Products;
using OceansApp.Models.ViewModels.ProductsClientsCompaniesAccountingConfig;
using OceansApp.Utility.SharedMethods.InputValidations;

namespace OceansAppWeb.Areas.AdminCenter.Controllers
{
    [ApiController]
    [Route("AdminCenter/[controller]")]
    [Area("AdminCenter")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    [Authorize]
    public class ProductsClientsCompaniesAccountingConfigForBillingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductsClientsCompaniesAccountingConfigForBillingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize(Policy = "AccessToCreateAndUpdateProductsClientsCompaniesAccountingConfig")]
        [HttpPost("CreateUpdateProductClientCompanyAccountingConfig")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateProductClientCompanyAccountingConfig([FromBody] CreateUpdateProductClientCompanyAccoutingConfigVM modelData)
        {
            try
            {
                if (modelData == null)
                {
                    return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
                }
                ValidateInputs validateInputs = new();

                validateInputs.ValidateRequiredFieldAnyValue("IsCreating", "Is Creating", modelData.IsCreating, ModelState);

                validateInputs.ValidateRequiredFieldAnyValue("ProductId", "Product", modelData.ProductId, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("ClientId", "Client", modelData.ClientId, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("CostCenterIdSales", "Cost Center Sales", modelData.CostCenterIdSales, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("CostCenterIdSalesDiscount", "Cost Center Sales Discount", modelData.CostCenterIdSalesDiscount, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("CostCenterIdSalesReturn", "Cost Center Sales Return", modelData.CostCenterIdSalesReturn, ModelState);

                validateInputs.ValidateRequiredFieldAnyValue("AccountingAccountIdSales", "Accounting Account Sales", modelData.AccountingAccountIdSales, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("AccountingAccountIdSalesDiscount", "Accounting Account Sales Discount", modelData.AccountingAccountIdSalesDiscount, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("AccountingAccountIdSalesReturn", "Accounting Account Sales Return", modelData.AccountingAccountIdSalesReturn, ModelState);

                if (modelData.TaxPercentage > 0)
                {
                    validateInputs.ValidateRequiredFieldAnyValue("CostCenterIdSalesTax", "Cost Center Sales Tax", modelData.CostCenterIdSalesTax, ModelState);
                    validateInputs.ValidateRequiredFieldAnyValue("AccountingAccountIdSalesTax", "Accounting Account Sales Tax", modelData.AccountingAccountIdSalesTax, ModelState);
                }


                if (ModelState.IsValid)
                {
                    if ((bool)modelData.IsCreating)
                    {
                        var res = await _unitOfWork.ProductClientCompanyAccountingConfig.CreateProductClientCompanyAccountingConfigAsync(modelData);

                        if (res.Success)
                        {
                            return Ok(res);
                        }
                        else
                        {
                            if (res.MessageType != "Validation Error")
                            {
                                return BadRequest(new { MessageType = res.MessageType, error = res.Message, result = "ErrorSaving", detail = $"The Accounting Config could not be saved." });
                            }
                            else
                            {
                                return BadRequest(new
                                {
                                    MessageType = res.MessageType,
                                    errors = new[] { res.Message }
                                });
                            }

                        }
                    }
                    else
                    {
                        //IF IS ID THEN UPDATE THE DEBIT/CREDIT
                        var res = await _unitOfWork.ProductClientCompanyAccountingConfig.UpdateProductClientCompanyAccountingConfigAsync(modelData);
                        if (res.Success)
                        {
                            return Ok(res);
                        }
                        else
                        {
                            if (res.MessageType != "Validation Error")
                            {
                                return BadRequest(new { error = res.Message, MessageType = res.MessageType, result = "ErrorSaving", detail = "The Accounting Configuration could not be updated." });
                            }
                            else
                            {
                                return BadRequest(new
                                {
                                    MessageType = res.MessageType,
                                    errors = new[] { res.Message }
                                });
                            }

                        }
                    }

                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();
                    return BadRequest(new { MessageType = "Validation Error", message = "Validation Error", result = "error", errors = errors });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { MessageType = "Exception Error", error = $"There was an error saving the changes. More details: " + ex.Message, detail = ex.Message });
            }
        }

    }
}
