using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;

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
                var productsList = await _unitOfWork.Product.SearchProjectsByTextWithAccountingConfigStatusAsync(searchText, clientId);

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
    }
}
