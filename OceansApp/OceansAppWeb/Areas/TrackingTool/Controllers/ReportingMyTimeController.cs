using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.ReportingMyTime;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Security.Claims;
using static OceansApp.Models.ViewModels.Components.MethodResponse;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Utility.LazyLoading;
using OceansApp.Models.ViewModels.Blobs;

namespace OceansAppWeb.Areas.TrackingTool.Controllers
{
    [Area("TrackingTool")]
    [Authorize]
    [Authorize(Policy = "BasicAccessToReportingMyTime")]
    [RequireTwoFactorEnabled]
    public class ReportingMyTimeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly LazyServiceProvider<IAzureBlobRepository> _azureBlobRepository;
        public ReportingMyTimeController(IUnitOfWork unitOrWork, LazyServiceProvider<IAzureBlobRepository> azureBlobRepository)
        {
            _unitOfWork = unitOrWork;
            _azureBlobRepository = azureBlobRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateTimeEntryClientNoTrackingTool([FromForm] List<IFormFile> files, [FromForm] List<CreateUpdateMovementClientNoTrackingToolVM> reportMovementListData)
        {
            if (reportMovementListData == null)
            {
                return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
            }
            ValidateInputs validateInputs = new();

            foreach (var movementTime in reportMovementListData)
            {
                validateInputs.ValidateNotRequiredAndStringLength("Notes", "Notes", movementTime.Notes, 200, ModelState);
                validateInputs.ValidateNonRequiredFieldIntType("MovementId", "MovementId " + movementTime.MovementType, movementTime.MovementId, ModelState);
                validateInputs.ValidateRequiredFieldIntType("ProjectId", "Project " + movementTime.MovementType, movementTime.ProjectId, ModelState);
                validateInputs.ValidateNoNegativeNumber("Quantity", "Quantity " + movementTime.MovementType, movementTime.Quantity, ModelState);
                validateInputs.ValidateNotRequiredFieldNumberValue("Quantity", "Quantity " + movementTime.MovementType, movementTime.Quantity, ModelState);
                validateInputs.ValidateDateValidFormat("ActionDate", "Action Date " + movementTime.MovementType, movementTime.ActionDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("ActionDate", "Action Date " + movementTime.MovementType, movementTime.ActionDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("MovementType", "MovementType", movementTime.MovementType, ModelState);
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
                MethodResponse result = null;
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

                    //Create the element
                    if (movementTime.MovementId == null && movementTime.MovementType == "Normal Hours"
                        || (movementTime.MovementId == null && movementTime.MovementType != "Normal Hours" && movementTime.Quantity > 0))
                    {
                        result = await _unitOfWork.ReportingMyTimeMovement.CreateTimeEntryClientNoTrackingTool(userActionedBy, elementToUpdateOrCreate);
                    }
                    else

                    //Update the element
                    if (movementTime.MovementId != null && movementTime.MovementType == "Normal Hours"
                        || (movementTime.MovementId != null && movementTime.MovementType != "Normal Hours" && movementTime.Quantity > 0))
                    {
                        result = await _unitOfWork.ReportingMyTimeMovement.UpdateTimeEntryClientNoTrackingTool(userActionedBy, elementToUpdateOrCreate);
                    }
                    else
                    //Delete the element
                    if (movementTime.MovementId != null && (movementTime.Quantity == 0 || movementTime.Quantity == null) && movementTime.MovementType != "Normal Hours")
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
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFilesClientNoTrackingTool([FromForm] List<IFormFile> files, int movementId)
        {
            ValidateInputs validateInputs = new();

            validateInputs.ValidateRequiredFieldIntType("MovementId", "MovementId", movementId, ModelState);
            validateInputs.ValidateRequiredFiles("Reports", "Reports", files, ModelState);
            validateInputs.ValidateValidFiles("Reports", files, ModelState);

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
                MethodResponse result = new MethodResponse();

                List<IFormFile> filesToUpload = await _unitOfWork.ReportingMyTimeMovement.VerifyIfUploadFile(files,movementId);

                string containerId = "consultant-hour-reports";
                List<BlobUploadResult> uploadedBlobs = await _azureBlobRepository.Value.UploadFilesAsync(containerId, filesToUpload, movementId);

                MethodResponse resultBlob = await _unitOfWork.ReportingMyTimeMovement.CreateReportingMyTimeMovementBlob(
                uploadedBlobs, movementId);

                if (!resultBlob.Success)
                {
                    return BadRequest(new { error = resultBlob.Message });
                }
                return Ok(new
                {
                    success = true,
                    message = resultBlob.Message,
                    fileNamesUploaded = resultBlob.StringsList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"An error occurred: {ex.Message}" });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMovementClientNoTrackingTool([FromForm] UploadFilesVM uploadFilesData)
        {
            if (uploadFilesData == null)
            {
                return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
            }
            ValidateInputs validateInputs = new();

            validateInputs.ValidateNonRequiredFieldIntType("MovementId", "MovementId", uploadFilesData.MovementId, ModelState);
            validateInputs.ValidateRequiredFieldIntType("ProjectId", "Project", uploadFilesData.ProjectId, ModelState);
            validateInputs.ValidateDateValidFormat("ActionDate", "Action Date", uploadFilesData.ActionDate, ModelState);
            validateInputs.ValidateRequiredFieldAnyValue("ActionDate", "Action Date", uploadFilesData.ActionDate, ModelState);

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
                MethodResponse result = new MethodResponse();

                CreateUpdateMovementClientNoTrackingToolVM movementToCreateCreate = new()
                {
                    MovementId = uploadFilesData.MovementId,
                    ProjectId = uploadFilesData.ProjectId,
                    Quantity = 0,
                    MovementType = "Normal Hours",
                    ActionDate = uploadFilesData.ActionDate
                };

                //Create the element
                if (uploadFilesData.MovementId == null)
                {
                    result = await _unitOfWork.ReportingMyTimeMovement.CreateTimeEntryClientNoTrackingTool(userActionedBy, movementToCreateCreate);
                    if (!result.Success)
                    {
                        return BadRequest(new { error = result.Message });
                    }
                }
                else
                {
                    result.IdCreatedElement = uploadFilesData.MovementId;
                }

                createdElementListToReturn.Add(new CreatedElement
                {
                    IdElement = result.IdCreatedElement
                });

                return Ok(new
                {
                    success = true,
                    createdMovementId = (int)result.IdCreatedElement,
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
