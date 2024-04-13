using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.ReportingMyTime;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Security.Claims;
using NodaTime;
using static OceansApp.Models.ViewModels.Components.MethodResponse;
using OceansApp.Models.ViewModels.Components;

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
        public async Task<IActionResult> CreateUpdateTimeEntryClientNoTrackingTool([FromForm] List<IFormFile> files, [FromForm] List<CreateUpdateMovementClientNoTrackingToolVM> reportMovementListData)
        {
            foreach (var file in files)
            {
                if (file.Length > 0)
                {

                }
            }

            if (reportMovementListData == null)
            {
                return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
            }
            ValidateInputs validateInputs = new();
            validateInputs.ValidateRequiredFiles("Reports", "Reports", files, ModelState);
            int isNormalHours = 0;
            foreach (var movementTime in reportMovementListData)
            {
                if (movementTime.MovementType == "Normal Hours")
                {
                    isNormalHours++;
                }
            }
            if (isNormalHours == 0)
            {
                ModelState.AddModelError("Normal Hours", "The total worked hours is required.");
            }
            foreach (var movementTime in reportMovementListData)
            {
                if (movementTime.MovementType == "Normal Hours")
                {
                    validateInputs.ValidateNotRequiredAndStringLength("Notes", "Notes", movementTime.Notes, 200, ModelState);
                    validateInputs.ValidateRequiredFieldNumberValue("Quantity", "Quantity" + movementTime.MovementType, movementTime.Quantity, ModelState);
                    validateInputs.ValidateNumberLessOrEqualThanZero("Quantity", "Quantity" + movementTime.MovementType, movementTime.Quantity, ModelState);
                }
                if (movementTime.MovementType == "Normal Hours" || movementTime.MovementId != null)
                {
                    validateInputs.ValidateNonRequiredFieldIntType("MovementId", "MovementId" + movementTime.MovementType, movementTime.MovementId, ModelState);
                    validateInputs.ValidateRequiredFieldIntType("ProjectId", "Project" + movementTime.MovementType, movementTime.ProjectId, ModelState);
                    validateInputs.ValidateNoNegativeNumber("Quantity", "Quantity" + movementTime.MovementType, movementTime.Quantity, ModelState);
                    validateInputs.ValidateDateValidFormat("ActionDate", "Action Date" + movementTime.MovementType, movementTime.ActionDate, ModelState);
                    validateInputs.ValidateRequiredFieldAnyValue("ActionDate", "Action Date" + movementTime.MovementType, movementTime.ActionDate, ModelState);
                    validateInputs.ValidateRequiredFieldAnyValue("MovementType", "MovementType", movementTime.MovementType, ModelState);
                }
            }

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
                List<CreatedElement> createdElementListToReturn = new List<CreatedElement>();
                foreach (var movementTime in reportMovementListData)
                {
                    CreateUpdateMovementClientNoTrackingToolVM elementToUpdateOrCreate = new()
                    {
                        MovementId = movementTime.MovementId,
                        ProjectId = movementTime.ProjectId,
                        Quantity = movementTime.Quantity,
                        MovementType = movementTime.MovementType,
                        ActionDate = movementTime.ActionDate,
                        Notes = movementTime.Notes
                    };
                    MethodResponse result = null;
                    //Create the element
                    if (movementTime.MovementId == null && movementTime.Quantity > 0)
                    {
                        result = await _unitOfWork.ReportingMyTimeMovement.CreateTimeEntryClientNoTrackingTool(userActionedBy, elementToUpdateOrCreate);
                    }else

                    //Update the element
                    if (movementTime.MovementId != null && movementTime.Quantity > 0)
                    {
                        result = await _unitOfWork.ReportingMyTimeMovement.UpdateTimeEntryClientNoTrackingTool(userActionedBy, elementToUpdateOrCreate);
                    }else
                    //Delete the element
                    if (movementTime.MovementId != null && movementTime.Quantity == 0)
                    {
                        result = await _unitOfWork.ReportingMyTimeMovement.DeleteTimeEntryClientNoTrackingTool((int)elementToUpdateOrCreate.MovementId);
                    }
                    else
                    {
                        result = new()
                        {
                            IdCreatedElement = null,
                            Message = "Changes Saved!",
                            Success = true
                        };
                    }

                    if (!result.Success)
                    {
                        return BadRequest(new { error = result.Message });
                    }
                    createdElementListToReturn.Add(new CreatedElement
                    {
                        IdElement = result.IdCreatedElement,
                        ElementType = movementTime.MovementType
                    });
                }
                return Ok(new
                {
                    success = true,
                    createdMovementList = createdElementListToReturn,
                    message = "Changes Saved!"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
