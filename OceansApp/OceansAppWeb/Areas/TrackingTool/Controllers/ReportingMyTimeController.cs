using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.ReportingMyTime;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Security.Claims;
using static OceansApp.Models.ViewModels.Components.MethodResponse;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Blobs;
using OceansApp.Models.ViewModels.ReportingMyTimeSubmissions;
using Microsoft.CodeAnalysis;
using OceansApp.Utility.SharedMethods;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Queues;
using Newtonsoft.Json;
using OceansApp.Utility.NotificationTemplates;
using System.Text;
using OceansApp.Models.ViewModels;
using Microsoft.ApplicationInsights;


namespace OceansAppWeb.Areas.TrackingTool.Controllers
{
    [ApiController]
    [Route("TrackingTool/[controller]")]
    [Area("TrackingTool")]
    [Authorize]
    [Authorize(Policy = "BasicAccessToReportingMyTime")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    public class ReportingMyTimeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Lazy<IAzureBlobRepository> _azureBlobRepository;
        private string _containerId;
        private readonly Lazy<QueueClient> _queueClient;
        private readonly TelemetryClient _telemetryClient;
        public ReportingMyTimeController(IUnitOfWork unitOrWork, Lazy<IAzureBlobRepository> azureBlobRepository,
            Lazy<QueueClient> queueClient, TelemetryClient telemetryClient)
        {
            _unitOfWork = unitOrWork;
            _azureBlobRepository = azureBlobRepository;
            _containerId = "consultant-hour-reports";
            _queueClient = queueClient;
            _telemetryClient = telemetryClient;
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // CLIENT HAS TRACKING TOOL - METHODS
        [HttpGet("GetProjectMovements")]
        public async Task<IActionResult> GetProjectMovements(int projectId, DateTime startDate, DateTime endDate)
        {
            try
            {
                ValidateInputs validateInputs = new();
                //Validate Filter inputs
                validateInputs.ValidateDateValidFormat("StartDate", "Start Date", startDate, ModelState);
                validateInputs.ValidateDateValidFormat("EndDate", "End Date", endDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("StartDate", "Start Date", startDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("EndDate", "End Date", endDate, ModelState);
                validateInputs.ValidateRequiredFieldIntType("ProjectId", "Project", projectId, ModelState);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Where(e => e.Value.Errors.Count > 0).ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                    return BadRequest(new { errors = errors });
                }
                string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var consultant = await _unitOfWork.ConsultantDetail.GetFirstOrDefaultAsync(x => x.UserId == userActionedBy);
                if (consultant == null)
                {
                    return NotFound(new { error = "Consultant does not exist." });
                }

                var totalResults = await _unitOfWork.ReportingMyTimeMovement.GetProjectMovementsAsync(projectId,
                    consultant.ConsultantId, startDate, endDate);

                var data = new { movementsList = totalResults };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error fetching project movements.", success = false, detail = ex.Message });
            }
        }

        [HttpPost("CreateUpdateTimeEntryClientNoTrackingTool")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateTimeEntryClientNoTrackingTool([FromForm] List<CreateUpdateMovementClientNoTrackingToolVM> reportMovementListData)
        {
            if (reportMovementListData == null)
            {
                return BadRequest(new { error = "The object data is null, it should be a valid object.", messageType = "Exception Error" });
            }
            ValidateInputs validateInputs = new();

            foreach (var movementTime in reportMovementListData)
            {
                validateInputs.ValidateNotRequiredAndStringLength("Notes", "Notes", movementTime.Notes, 400, ModelState);
                validateInputs.ValidateRequiredFieldIntType("ProjectId", "Project " + movementTime.MovementType, movementTime.ProjectId, ModelState);
                validateInputs.ValidateNoNegativeNumber("Quantity", "Quantity " + movementTime.MovementType, movementTime.Quantity, ModelState);
                validateInputs.ValidateNotRequiredAndGreaterThanZeroFieldNumberValue("Quantity", "Quantity " + movementTime.MovementType, movementTime.Quantity, ModelState);
                validateInputs.ValidateLengthTypeNumber("Quantity", "Quantity " + movementTime.MovementType, movementTime.Quantity, 18, 2, ModelState);
                validateInputs.ValidateDateValidFormat("ActionDate", "Action Date " + movementTime.MovementType, movementTime.ActionDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("ActionDate", "Action Date " + movementTime.MovementType, movementTime.ActionDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("StartActionDate", "Start Action Date " + movementTime.MovementType, movementTime.StartActionDate, ModelState);
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
                return BadRequest(new { errors = errors, messageType = "Validation Error" });
            }

            try
            {
                MethodResponse result = null;
                int movementId = 0;
                string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                foreach (var movementTime in reportMovementListData)
                {
                    var movementType = await _unitOfWork.ReportingMyTimeMovementType.GetFirstOrDefaultAsync(x => x.Name == movementTime.MovementType);
                    if (movementType == null)
                    {
                        return BadRequest(new { error = "Movement Type does not exist.", messageType = "Exception Error" });
                    }
                    CreateUpdateMovementClientNoTrackingToolVM elementToUpdateOrCreate = new()
                    {
                        ProjectId = movementTime.ProjectId,
                        Quantity = movementTime.Quantity,
                        MovementType = movementTime.MovementType,
                        MovementTypeId = movementType.MovementTypeId,
                        StartActionDate = movementTime.StartActionDate,
                        ActionDate = movementTime.ActionDate,
                        Notes = movementTime.Notes
                    };

                    //Look for existing movement
                    MethodResponse resultExistingMovement = await _unitOfWork.ReportingMyTimeMovement.GetExistingMovement(userActionedBy, elementToUpdateOrCreate);
                    if (!resultExistingMovement.Success)
                    {
                        return BadRequest(new { error = resultExistingMovement.Message, messageType = resultExistingMovement.MessageType });
                    }

                    //Create the element
                    if (resultExistingMovement.IdCreatedElement == null && movementTime.MovementType == "Normal Hours"
                        || (resultExistingMovement.IdCreatedElement == null && movementTime.MovementType != "Normal Hours" && movementTime.Quantity > 0))
                    {
                        result = await _unitOfWork.ReportingMyTimeMovement.CreateTimeEntryClientNoTrackingTool(userActionedBy, elementToUpdateOrCreate);
                        if (movementTime.MovementType == "Normal Hours")
                        {
                            movementId = (int)result.IdCreatedElement;
                        }
                    }
                    else
                    //Update the element
                    if (resultExistingMovement.IdCreatedElement != null && movementTime.MovementType == "Normal Hours"
                        || (resultExistingMovement.IdCreatedElement != null && movementTime.MovementType != "Normal Hours" && movementTime.Quantity > 0))
                    {
                        elementToUpdateOrCreate.MovementId = resultExistingMovement.IdCreatedElement;
                        result = await _unitOfWork.ReportingMyTimeMovement.UpdateTimeEntryClientNoTrackingTool(userActionedBy, elementToUpdateOrCreate);
                    }
                    else
                    //Delete the element
                    if (resultExistingMovement.IdCreatedElement != null && (movementTime.Quantity == 0 || movementTime.Quantity == null) && movementTime.MovementType != "Normal Hours")
                    {
                        result = await _unitOfWork.ReportingMyTimeMovement.DeleteTimeEntryClientNoTrackingTool((int)resultExistingMovement.IdCreatedElement);
                    }
                    else
                    {
                        result = new()
                        {
                            IdCreatedElement = null,
                            Message = "Changes Saved!",
                            Success = true,
                            MessageType = "Exception Error"
                        };
                    }
                    if (resultExistingMovement.IdCreatedElement != null && movementTime.MovementType == "Normal Hours")
                    {
                        movementId = (int)resultExistingMovement.IdCreatedElement;
                    }

                    if (!result.Success)
                    {
                        return BadRequest(new { error = result.Message, messageType = result.MessageType });
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    movementIdNormalHours = movementId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"An error occurred: {ex.Message}", messageType = "Exception Error" });
            }
        }

        [HttpPost("UploadFilesClientNoTrackingTool")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFilesClientNoTrackingTool([FromForm] List<IFormFile> files, [FromForm] int movementId)
        {
            try
            {
                ValidateInputs validateInputs = new();

                validateInputs.ValidateRequiredFieldIntType("MovementId", "MovementId", movementId, ModelState);
                validateInputs.ValidateRequiredFiles("Reports", "Reports", files, ModelState);
                validateInputs.ValidateValidFiles("Reports", files, ModelState);

                int? numUploadedFilesInMovement = await _unitOfWork.ReportingMyTimeMovement.VerifyNumUploadedFilesPerMovementAsync(movementId);

                if (numUploadedFilesInMovement == null)
                {
                    return BadRequest(new { error = "Something went wrong getting the num of uploaded files.", messageType = "Exception Error" });
                }
                if (numUploadedFilesInMovement > 7)
                {
                    ModelState.AddModelError("Reports", "You can not upload more than 8 files.");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
                    return BadRequest(new { errors = errors, messageType = "Validation Error" });
                }

                var movement = await _unitOfWork.ReportingMyTimeMovement.GetFirstOrDefaultAsync(x => x.MovementId == movementId, x => x.TransactionStatus);
                if (movement == null)
                {
                    return BadRequest(new { error = "Movement does not exist.", messageType = "Exception Error" });
                }

                MethodResponse responseValidateSubmission = await _unitOfWork.ReportingMyTimeMovement.ValidateSubmission(movement, null,
                    null, null);

                if (!responseValidateSubmission.Success)
                {
                    return BadRequest(new { error = responseValidateSubmission.Message, messageType = responseValidateSubmission.MessageType });
                }

                List<IFormFile> filesToUpload = await _unitOfWork.ReportingMyTimeMovement.VerifyIfUploadFile(files, movementId);

                List<BlobUploadResult> uploadedBlobs = await _azureBlobRepository.Value.UploadFilesAsync(_containerId, filesToUpload, movementId.ToString(), 2800);

                MethodResponse resultBlob = await _unitOfWork.ReportingMyTimeMovement.CreateReportingMyTimeMovementBlob(
                uploadedBlobs, movementId);

                if (!resultBlob.Success)
                {
                    return BadRequest(new { error = resultBlob.Message, messageType = "Exception Error" });
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

        [HttpPost("CreateMovementClientNoTrackingTool")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMovementClientNoTrackingTool([FromForm] UploadFilesVM uploadFilesData)
        {
            if (uploadFilesData == null)
            {
                return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
            }
            ValidateInputs validateInputs = new();

            validateInputs.ValidateRequiredFieldIntType("ProjectId", "Project", uploadFilesData.ProjectId, ModelState);
            validateInputs.ValidateDateValidFormat("ActionDate", "Action Date", uploadFilesData.ActionDate, ModelState);
            validateInputs.ValidateRequiredFieldAnyValue("ActionDate", "Action Date", uploadFilesData.ActionDate, ModelState);
            validateInputs.ValidateRequiredFieldAnyValue("StartActionDate", "Start Action Date", uploadFilesData.StartActionDate, ModelState);

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

                var movementType = await _unitOfWork.ReportingMyTimeMovementType.GetFirstOrDefaultAsync(x => x.Name == "Normal Hours");
                if (movementType == null)
                {
                    return NotFound(new { error = "Movement Type not found" });
                }

                List<CreatedElement> createdElementListToReturn = new List<CreatedElement>();
                MethodResponse result = new MethodResponse();

                CreateUpdateMovementClientNoTrackingToolVM movementToCreateCreate = new()
                {
                    ProjectId = uploadFilesData.ProjectId,
                    Quantity = 0,
                    MovementType = "Normal Hours",
                    MovementTypeId = movementType.MovementTypeId,
                    ActionDate = uploadFilesData.ActionDate,
                    StartActionDate = uploadFilesData.StartActionDate
                };

                //Look for existing movement
                MethodResponse resultExistingMovement = await _unitOfWork.ReportingMyTimeMovement.GetExistingMovement(userActionedBy, movementToCreateCreate);
                if (!resultExistingMovement.Success)
                {
                    return BadRequest(new { error = resultExistingMovement.Message });
                }

                //Create the element
                if (resultExistingMovement.IdCreatedElement == null)
                {
                    result = await _unitOfWork.ReportingMyTimeMovement.CreateTimeEntryClientNoTrackingTool(userActionedBy, movementToCreateCreate);
                    if (!result.Success)
                    {
                        return BadRequest(new { error = result.Message });
                    }
                }

                return Ok(new
                {
                    success = true,
                    createdMovementId = (int)result.IdCreatedElement
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("DeleteBlob")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBlob([FromForm] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest("File name must be provided.");
            }
            MethodResponse response = await _azureBlobRepository.Value.DeleteBlobAsync(_containerId, fileName);

            if (response.Success)
            {
                MethodResponse deleteFileFromDb = await _unitOfWork.ReportingMyTimeMovement.DeleteBlobReport(fileName);
                if (!deleteFileFromDb.Success)
                {
                    return BadRequest(deleteFileFromDb.Message);
                }
                return Ok(new { success = true, message = response.Message });
            }
            else
            {
                return BadRequest(response.Message);
            }
        }

        // CLIENT DOES NOT HAVE TRACKING TOOL - METHODS
        [HttpPost("CreateUpdateTimeEntryTrackingTool")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateTimeEntryTrackingTool([FromBody] CreateUpdateMovementTrackingToolVM timeEntryData)
        {
            if (timeEntryData == null)
            {
                return BadRequest(new { error = "The object data is null, it should be a valid object.", messageType = "Exception Error" });
            }
            ValidateInputs validateInputs = new();
            double totalQuantity = DateAndTimes.CalculateNumHours(timeEntryData.TimeFrom, timeEntryData.TimeTo);
            if (totalQuantity == 0)
            {
                return BadRequest(new { MessageType = "Validation Error", errors = new[] { $"You cannot add time equals to zero." } });
            }
            validateInputs.ValidateNotRequiredAndStringLength("Notes", "Notes", timeEntryData.Notes, 400, ModelState);
            validateInputs.ValidateRequiredAndStringLength("TimeFrom", "Time From", timeEntryData.TimeFrom, 5, ModelState);
            validateInputs.ValidateRequiredAndStringLength("TimeTo", "Time To", timeEntryData.TimeTo, 5, ModelState);
            validateInputs.ValidateRequiredFieldIntType("ProjectId", "Project", timeEntryData.ProjectId, ModelState);
            validateInputs.ValidateDateValidFormat("ActionDate", "Action Date", timeEntryData.ActionDate, ModelState);
            validateInputs.ValidateRequiredFieldAnyValue("ActionDate", "Action Date", timeEntryData.ActionDate, ModelState);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(e => e.Value.Errors.Count > 0).ToDictionary(kvp => kvp.Key, kvp =>
                kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

                return BadRequest(new { errors = errors, messageType = "Validation Error" });
            }

            try
            {
                MethodResponse result = null;
                int movementId = 0;

                string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                //Create the element
                if (timeEntryData.MovementId == null)
                {
                    result = await _unitOfWork.ReportingMyTimeMovement.CreateTimeEntryTrackingTool(userActionedBy, timeEntryData);
                }
                else //Update the element
                {
                    result = await _unitOfWork.ReportingMyTimeMovement.UpdateTimeEntryTrackingTool(userActionedBy, timeEntryData);
                }

                if (!result.Success)
                {
                    return BadRequest(new { error = result.Message, messageType = result.MessageType });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    movementId = result.IdCreatedElement
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"An error occurred: {ex.Message}", messageType = "Exception Error" });
            }
        }

        [HttpGet("GetTrackingToolProjectMovements")]
        public async Task<IActionResult> GetTrackingToolProjectMovements(int projectId, DateTime startDate, DateTime endDate)
        {
            try
            {
                ValidateInputs validateInputs = new();
                //Validate Filter inputs
                validateInputs.ValidateDateValidFormat("StartDate", "Start Date", startDate, ModelState);
                validateInputs.ValidateDateValidFormat("EndDate", "End Date", endDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("StartDate", "Start Date", startDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("EndDate", "End Date", endDate, ModelState);
                validateInputs.ValidateRequiredFieldIntType("ProjectId", "Project", projectId, ModelState);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Where(e => e.Value.Errors.Count > 0).ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                    return BadRequest(new { errors = errors });
                }
                string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var consultant = await _unitOfWork.ConsultantDetail.GetFirstOrDefaultAsync(x => x.UserId == userActionedBy);
                if (consultant == null)
                {
                    return NotFound(new { error = "Consultant does not exist." });
                }

                var totalResults = await _unitOfWork.ReportingMyTimeMovement.GetTrackingToolProjectMovementsAsync(projectId,
                    consultant.ConsultantId, startDate, endDate);

                var data = new { movementsList = totalResults };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error fetching project movements.", success = false, detail = ex.Message });
            }
        }

        [HttpPost("DeleteTrackingToolTimeEntry")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTrackingToolTimeEntry([FromForm] int movementId)
        {
            if (movementId == null)
            {
                return BadRequest(new { error = "MovementId is required", messageType = "Validation Error" });
            }

            try
            {
                string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userActionedBy == null)
                {
                    return BadRequest(new { error = "User does not exist.", messageType = "Exception Error" });
                }

                MethodResponse response = await _unitOfWork.ReportingMyTimeMovement.DeleteTrackingTooTimeEntry(userActionedBy, movementId);
                if (!response.Success)
                {
                    if (response.MessageType == "Not Found")
                    {
                        return NotFound(new { error = response.Message, messageType = response.MessageType });
                    }
                    return BadRequest(new { error = response.Message, messageType = response.MessageType });
                }
                return Ok(new { success = true, message = response.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, messageType = "Exception Error" });
            }
        }

        // SUBMIT REPORT
        [HttpPost("SubmitReport")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReport([FromBody] CreateSubmissionVM submissionData)
        {
            if (submissionData == null)
            {
                return BadRequest(new { error = "The object data is null, it should be a valid object.", messageType = "Exception Error" });
            }
            ValidateInputs validateInputs = new();

            validateInputs.ValidateDateValidFormat("StartPeriodDate", "Start Period Date", submissionData.StartPeriodDate, ModelState);
            validateInputs.ValidateDateValidFormat("EndPeriodDate", "End Period Date", submissionData.EndPeriodDate, ModelState);

            if (!ModelState.IsValid)
            {
                var errors = ModelState
        .Where(e => e.Value.Errors.Count > 0)
        .ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
        );
                return BadRequest(new { errors = errors, messageType = "Validation Error" });
            }

            try
            {
                MethodResponse result = null;
                int movementId = 0;
                string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                //Create the element
                result = await _unitOfWork.ReportingMyTimeMovementSubmission.CreateSubmission(userActionedBy, submissionData);

                if (!result.Success)
                {
                    if (result.MessageType == "Validation Error")
                    {
                        var errors = new Dictionary<string, List<string>>
        {
            { result.FieldName, new List<string> { result.Message } }
        };
                        return BadRequest(new { errors = errors, messageType = "Validation Error" });
                    }

                    return BadRequest(new { error = result.Message, messageType = result.MessageType });
                }
                //Send notification

                return Ok(new
                {
                    success = true,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"An error occurred: {ex.Message}", messageType = "Exception Error" });
            }
        }

        [HttpGet("GetTrackingToolMovementDataById")]
        public async Task<IActionResult> GetTrackingToolMovementDataById(int movementId)
        {
            try
            {
                var existingMovement = await _unitOfWork.ReportingMyTimeMovement.GetFirstOrDefaultAsync(x => x.MovementId == movementId,
                    include: x => x.Include(y => y.ReportingMyTimeMovementType));
                if (existingMovement == null)
                {
                    return NotFound(new { error = "Movement does not exist.", messageType = "Not Found" });
                }
                GetTrackingToolMovementDataVM vm = new()
                {
                    ActionDate = existingMovement.ActionDate,
                    Notes = existingMovement.Notes,
                    TimeFrom = existingMovement.TimeFrom,
                    TimeTo = existingMovement.TimeTo,
                    MovementTypeId = existingMovement.MovementTypeId,
                    MovementTypeName = existingMovement.ReportingMyTimeMovementType.Name
                };
                var data = new { movementData = vm };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error fetching the data.", success = false, detail = ex.Message });
            }
        }


        //NO HTTP METHODS
        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task<MethodResponse> SendNotificationSubmitReport(string consultantName, string consultantEmail)
        {
            try
            {
                var emailTemplates = new EmailTemplates();
                var createPassBody = emailTemplates.CreatePasswordBody("Hola", consultantName.Trim());
                var templateEmail = emailTemplates.EmailTemplate("CREATE YOUR PASSWORD", createPassBody);

                SendEmailVM emailToSend = new()
                {
                    Subject = "Create your account - Oceans App",
                    SharedEmailFrom = Environment.GetEnvironmentVariable("sharedEmailOceansApp"),
                    EmailTo = consultantEmail.Trim(),
                    Body = templateEmail
                };

                var messageContent = JsonConvert.SerializeObject(emailToSend);
                try
                {
                    await _queueClient.Value.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(messageContent)));
                }
                catch (Exception queueEx)
                {
                    _telemetryClient.TrackTrace($"Fail sending email to: {consultantEmail.Trim()}, the connection with the function app failed");
                }

                return new MethodResponse { Success = true, Message = "Notification sent." };
            }
            catch (Exception ex)
            {
                return new MethodResponse { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }




    }
}
