using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.DocumentsCCSubtypes;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Security.Claims;

namespace OceansAppWeb.Areas.Finances.Controllers
{
    [ApiController]
    [Route("Finances/[controller]")]
    [Area("Finances")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    [Authorize]
    [Authorize(Policy = "AccessToAccountsReceivable")]
    public class DocumentCCSubtypesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public DocumentCCSubtypesController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        [HttpGet("GetDocumentCCSubtypesList")]
        public async Task<IActionResult> GetDocumentCCSubtypesList()
        {
            try
            {
                var data = await _unitOfWork.DocumentCCSubtype.GetDocumentSubtypesListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"There was an error fetching the list of subtypes." }, success = false, result = "errorGet", detail = ex.Message });
            }
        }

        [HttpPost("CreateUpdateDocumentSubtype")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateDocumentSubtype([FromBody] CreateUpdateDocumentSubtypeVM docSubtypeData)
        {
            try
            {
                if (docSubtypeData == null)
                {
                    return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
                }
                ValidateInputs validateInputs = new();

                validateInputs.ValidateRequiredAndStringLength("Description", "Description", docSubtypeData.Description, 25, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("DocumentType", "Document Type", docSubtypeData.DocumentTypeId, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("CompanyId", "Company", docSubtypeData.CompanyId, ModelState);
                validateInputs.ValidateRequiredFieldIntType("CostCenterId", "Cost Center", docSubtypeData.CostCenterId, ModelState);
                validateInputs.ValidateRequiredFieldIntType("AccountingAccountId", "Accounting Account", docSubtypeData.AccountingAccountId, ModelState);


                if (ModelState.IsValid)
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var resultMessage = "";
                    var userActionedBy = claim.Value;

                    //IF IS NOT ID THEN CREATE IT
                    if (docSubtypeData.DocumentCCSubtypeId == null)
                    {
                        var res = await _unitOfWork.DocumentCCSubtype.CreateDocumentSubType(docSubtypeData);

                        if (res.Success)
                        {
                            resultMessage = res.Message;
                        }
                        else
                        {
                            if (res.MessageType != "Validation Error")
                            {
                                return BadRequest(new { MessageType = res.MessageType, error = res.Message, result = "ErrorSaving", detail = $"The Documenty Subtype could not be saved." });
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
                        //IF IS ID THEN UPDATE THE DOCUMENT SUBTYPE
                        var res = await _unitOfWork.DocumentCCSubtype.UpdateDocumentSubtype(docSubtypeData);
                        if (res.Success)
                        {
                            resultMessage = res.Message;
                        }
                        else
                        {
                            if (res.MessageType != "Validation Error")
                            {
                                return BadRequest(new { error = res.Message, MessageType = res.MessageType, result = "ErrorSaving", detail = "The Document Type could not be updated." });
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
                    return Ok(new
                    {
                        success = true,
                        message = resultMessage
                    });
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

        [HttpGet("GetDocumentSubtypeDataById")]
        public async Task<IActionResult> GetDocumentSubtypeDataById(int docSubtypeId)
        {
            try
            {
                var docSubtypeData = await _unitOfWork.DocumentCCSubtype.GetDocumentSubtypeByIdAsync(docSubtypeId);
                if (docSubtypeData == null)
                {
                    return BadRequest(new { error = "The Document Subtype is not longer in the database." });
                }

                return Ok(new
                {
                    documentSubtypeData = docSubtypeData
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
    }
}
