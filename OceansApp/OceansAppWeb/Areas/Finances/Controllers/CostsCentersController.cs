using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;

namespace OceansAppWeb.Areas.Finances.Controllers
{
    [Area("Finances")]
    [Authorize]
    [RequireTwoFactorEnabled]
    public class CostsCentersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CostsCentersController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        [Authorize(Policy = "AccessToCostsCentersList")]
        [HttpGet]
        public async Task<IActionResult> GetCostsCentersListWhereCompanyId(string companyId)
        {
            try
            {
                var costsCenters = await _unitOfWork.CenterOfCosts.GetCostsCentersWhereCompanyIdAsync(companyId);
                return Ok(new
                {
                    CostsCenters = costsCenters
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
    }
}
