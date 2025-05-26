using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Products;
using OceansApp.Utility.SharedMethods.InputValidations;

namespace OceansAppWeb.Areas.AdminCenter.Controllers
{
    [ApiController]
    [Route("AdminCenter/[controller]")]
    [Area("AdminCenter")]
    [EnableCors("AllowSpecificOrigin")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize(Policy = "AccessToSearchForProjectsList")]
        [HttpGet("SearchProjectsByTextWithAccountingConfigStatus")]
        public async Task<IActionResult> SearchProjectsByTextWithAccountingConfigStatus(string searchText, int clientId)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return BadRequest(new { error = "The search text is required." });

            if (clientId <= 0)
                return BadRequest(new { error = "A valid client ID must be provided." });
            try
            {
                var productsList = await _unitOfWork.Product.SearchProductsByTextWithAccountingConfigStatusAsync(searchText, clientId);

                return Ok(new
                {
                    productsList = productsList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
 
        [Authorize(Policy = "AccessToCreateAndUpdateProducts")]
        [HttpPost("CreateUpdateProduct")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateProduct([FromBody] CreateUpdateProductVM projectData)
        {
            try
            {
                if (projectData == null)
                {
                    return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
                }
                ValidateInputs validateInputs = new();

                validateInputs.ValidateRequiredAndStringLength("Name", "Product Name", projectData.ProductName, 150, ModelState);
                validateInputs.ValidateRequiredAndStringLength("Alias", "Product Alias", projectData.Alias, 150, ModelState);
                validateInputs.ValidateNotRequiredAndStringLength("Detail", "Detail", projectData.Detail, 300, ModelState);


                if (ModelState.IsValid)
                {
                    //IF IS NOT ID THEN CREATE IT
                    if (projectData.ProductId == null)
                    {
                        var res = await _unitOfWork.Product.CreateProductAsync(projectData);

                        if (res.Success)
                        {
                            return Ok(res);
                        }
                        else
                        {
                            if (res.MessageType != "Validation Error")
                            {
                                return BadRequest(new { MessageType = res.MessageType, error = res.Message, result = "ErrorSaving", detail = $"The Product could not be saved." });
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
                        var res = await _unitOfWork.Product.CreateProductAsync(projectData);
                        if (res.Success)
                        {
                            return Ok(res);
                        }
                        else
                        {
                            if (res.MessageType != "Validation Error")
                            {
                                return BadRequest(new { error = res.Message, MessageType = res.MessageType, result = "ErrorSaving", detail = "The Product could not be updated." });
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
