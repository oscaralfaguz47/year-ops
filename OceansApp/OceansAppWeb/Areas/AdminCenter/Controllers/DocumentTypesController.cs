using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;

namespace OceansAppWeb.Areas.AdminCenter.Controllers
{
    [ApiController]
    [Route("AdminCenter/[controller]")]
    [Area("AdminCenter")]
    [EnableCors("AllowSpecificOrigin")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    [Authorize]
    public class DocumentTypesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public DocumentTypesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize(Policy = "AccessToAllDocumentTypesList")]
        [HttpGet("GetAllDocumentTypesListForSelect")]
        public async Task<IActionResult> GetAllDocumentTypesListForSelect()
        {
            try
            {
                List<SelectVM> documentTypesList = new();
                var documentTypes = await _unitOfWork.DocumentType.GetAllAsync();
                foreach (var docType in documentTypes)
                {
                    documentTypesList.Add(new SelectVM { Value = docType.DocumentTypeId, Text = docType.Description });
                }
                return Ok(new
                {
                    DocumentTypes = documentTypesList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToAllDocumentTypesList")]
        [HttpGet("GetAllDocumentTypesListByTransactionType")]
        public async Task<IActionResult> GetAllDocumentTypesListByTransactionType(int transactionTypeId)
        {
            try
            {
                List<SelectVM> documentTypesList = new();
                var documentTypes = await _unitOfWork.DocumentType.GetAllAsync(x => x.TransactionTypeId == transactionTypeId);
                foreach (var docType in documentTypes)
                {
                    documentTypesList.Add(new SelectVM { Value = docType.DocumentTypeId, Text = docType.Description });
                }
                return Ok(new
                {
                    DocumentTypes = documentTypesList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
    }
}
