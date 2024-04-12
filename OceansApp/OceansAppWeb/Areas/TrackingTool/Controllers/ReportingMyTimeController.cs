using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.ReportingMyTime;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Security.Claims;
using NodaTime;

namespace OceansAppWeb.Areas.TrackingTool.Controllers
{
    [Area("TrackingTool")]
    [Authorize]
    [Authorize(Policy = "BasicAccessToReportingMyTime")]
    [RequireTwoFactorEnabled]
    public class ReportingMyTimeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReportingMyTimeController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateTimeEntryClientNoTrackingTool([FromForm] List<IFormFile> files, [FromForm] CreateUpdateMovementClientNoTrackingToolVM reportMovementData)
        {
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                  
                }
            }

            if (reportMovementData == null)
            {
                return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
            }
            ValidateInputs validateInputs = new();
            validateInputs.ValidateRequiredFiles("Reports", "Reports", files, ModelState);
            validateInputs.ValidateNonRequiredFieldIntType("MovementId", "MovementId", reportMovementData.MovementId, ModelState);
            validateInputs.ValidateRequiredFieldIntType("ProjectId", "Project", reportMovementData.ProjectId, ModelState);
            validateInputs.ValidateRequiredFieldNumberValue("Quantity", "Quantity", reportMovementData.Quantity, ModelState);
            validateInputs.ValidateNoNegativeNumber("Quantity", "Quantity", reportMovementData.Quantity, ModelState);
            validateInputs.ValidateNumberLessOrEqualThanZero("Quantity", "Quantity", reportMovementData.Quantity, ModelState);
            validateInputs.ValidateDateValidFormat("ActionDate", "Action Date", reportMovementData.ActionDate, ModelState);
            validateInputs.ValidateRequiredFieldAnyValue("ActionDate", "Action Date", reportMovementData.ActionDate, ModelState);
            validateInputs.ValidateNotRequiredAndStringLength("Notes", "Notes", reportMovementData.Notes, 200, ModelState);

            if (!ModelState.IsValid)
            {
                var errors = ModelState
        .Where(e => e.Value.Errors.Count > 0)
        .ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
        );
                return BadRequest(new { errors = errors });
            }

            try
            {
                string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var result = reportMovementData.MovementId == null ?
                await _unitOfWork.ReportingMyTimeMovement.CreateTimeEntryClientNoTrackingTool(userActionedBy, reportMovementData) :
                await _unitOfWork.ReportingMyTimeMovement.UpdateTimeEntryClientNoTrackingTool(userActionedBy, reportMovementData);

                if (!result.Success)
                {
                    return BadRequest(new { error = result.Message });
                }

                return Ok(new
                {
                    success = true,
                    createdMovement = result.IdCreatedElement,
                    message = result.Message
                });

            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
