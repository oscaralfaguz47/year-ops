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
    public class MovementTypesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public MovementTypesController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        [Authorize(Policy = "BasicAccessToReportingMyTime")]
        [HttpGet("GetMovementTypeForTrackingTool")]
        public async Task<IActionResult> GetMovementTypeForTrackingTool()
        {
            try
            {
                List<GetDataForSelectVM> movementTypesList = new();
                var movementTypes = await _unitOfWork.ReportingMyTimeMovementType.GetAllAsync(x => x.IsPayable == false || 
                x.Name == "Normal Hours");
                foreach (var movementType in movementTypes)
                {
                    movementTypesList.Add(new GetDataForSelectVM { Value = movementType.MovementTypeId, Text = movementType.Name });
                }
                return Ok(new
                {
                    MovementTypes = movementTypesList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
    }
}
