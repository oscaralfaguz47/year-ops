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
    [RequireTwoFactorEnabled]
    [Authorize]
    public class ConsultantPositionsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ConsultantPositionsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        [Authorize(Policy = "AccessToAllConsultantPositionsList")]
        [HttpGet("GetAllConsultantPositionsListForSelect")]
        public async Task<IActionResult> GetAllConsultantPositionsListForSelect(bool isAdministrative)
        {
            try
            {
                List<GetDataForSelectVM> positionsList = await _unitOfWork.ConsultantPosition.GetPositionsByIsAdministrative(isAdministrative);

                return Ok(new
                {
                    Positions = positionsList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
    }
}
