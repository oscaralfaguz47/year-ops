using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;

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
                var data = await _unitOfWork.DocumentCCSubtype.GetAllAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"There was an error fetching the list of subtypes." }, success = false, result = "errorGet", detail = ex.Message });
            }
        }
    }
}
