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
    public class ReportingMyTimeMovementTypesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReportingMyTimeMovementTypesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize(Policy = "AccessToReportingMyTimeMovementTypesListForSelect")]
        [HttpGet("GetMovementTypesListForSelect")]
        public async Task<IActionResult> GetMovementTypesListForSelect()
        {
            try
            {
                List<SelectVM> movementTypesList = new();
                var movementTypes = await _unitOfWork.ReportingMyTimeMovementType.GetAllAsync(x => x.Name == "Normal Hours" || x.Name == "On Call Flate Rate" || x.Name == "On Call Time Worked" || x.Name == "Overtime Hours");
                foreach (var movementType in movementTypes)
                {
                    movementTypesList.Add(new SelectVM { Value = movementType.MovementTypeId.ToString(), Text = movementType.Name });
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
