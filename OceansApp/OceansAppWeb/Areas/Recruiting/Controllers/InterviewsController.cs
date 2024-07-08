using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Interviews;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Security.Claims;

namespace OceansAppWeb.Areas.Recruiting.Controllers
{
    [ApiController]
    [Route("Recruiting/[controller]")]
    [Area("Recruiting")]
    [Authorize]
    [Authorize(Policy = "AccessToManageInterviews")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    public class InterviewsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        public InterviewsController(IUnitOfWork unitOrWork, IConfiguration config)
        {
            _unitOfWork = unitOrWork;
            _config = config;
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetInterviewsList")]
        public async Task<IActionResult> GetInterviewsList(string model)
        {
            try
            {
                if (model != "{}")
                {
                    JObject jsonToValidate = JObject.Parse(model);
                    if (jsonToValidate["Filters"] == null || jsonToValidate["PaginationWithoutFilters"] == null)
                    {
                        return BadRequest(new { errors = new[] { "You should pass a valid Json like: {Filters: null, PaginationWithoutFilters:null}" }, result = "errorGet", detail = "The json is invalid." });
                    }
                    else
                    {
                        if (jsonToValidate["Filters"] != null)
                        {
                            ValidateInputs validateInputs = new();
                            //Validate Filter inputs
                            validateInputs.ValidateNotRequiredAndStringLength("SearchText", "Search Text", jsonToValidate["Filters"]["SearchText"].ToString(), 100, ModelState);
                            validateInputs.ValidateDateValidFormat("StartDate", "Start Date", jsonToValidate["Filters"]["StartDate"], ModelState);
                            validateInputs.ValidateDateValidFormat("EndDate", "End Date", jsonToValidate["Filters"]["EndDate"], ModelState);
                            validateInputs.ValidateNonRequiredFieldIntType("TransactionStatusId", "Transaction Status", (int?)jsonToValidate["Filters"]["TransactionStatusId"], ModelState);

                            if (!ModelState.IsValid)
                            {
                                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                              .Select(e => e.ErrorMessage)
                                                              .ToList();
                                return BadRequest(new { MessageType = "Validation Error", message = "Validation Error", result = "error", errors = errors, detail = "Parameters for filters are not correct." });
                            }
                        }
                    }
                }

                InterviewsPaginationFiltersVM interviewsPaginationFilters = System.Text.Json.JsonSerializer.Deserialize<InterviewsPaginationFiltersVM>(model);

                InterviewsPaginationFiltersVM paginationFilters = new();
                paginationFilters.Filters = new InterviewsFiltersGetAllVM();

                int numAppliedFilters = 0;
                if (interviewsPaginationFilters.Filters != null)
                {
                    foreach (var prop in interviewsPaginationFilters.Filters.GetType().GetProperties())
                    {
                        var value = prop.GetValue(interviewsPaginationFilters.Filters, null);
                        if (value is not null and not "")
                        {
                            numAppliedFilters++;
                        }
                    }
                }
                var setPagination = new PaginationFiltersBehavior();
                paginationFilters.PaginationWithoutFilters = setPagination.SetPagination(interviewsPaginationFilters.PaginationWithoutFilters, numAppliedFilters);

                if (numAppliedFilters > 0)
                {
                    paginationFilters.Filters = interviewsPaginationFilters.Filters;
                }

                var totalResults = await _unitOfWork.Interview.GetAllInterviewsWithFiltersAsync(paginationFilters);
                paginationFilters.PaginationWithoutFilters.Pagination.TotalResults = totalResults.totalCount;

                var data = new { InterviewsList = totalResults.interviews, PaginationFilters = paginationFilters };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"There was an error fetching the list of interviews." }, success = false, result = "errorGet", detail = ex.Message });
            }
        }

        [HttpPost("CreateUpdateInterview")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateInterview([FromBody] CreateUpdateInterviewVM interviewData)
        {
            try
            {
                if (interviewData == null)
                {
                    return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
                }
                ValidateInputs validateInputs = new();

                validateInputs.ValidateNonRequiredFieldIntType("InterviewId", "InterviewId", interviewData.InterviewId, ModelState);
                validateInputs.ValidateRequiredFieldIntType("ConsultantId", "Consultant", interviewData.ConsultantId, ModelState);
                validateInputs.ValidateRequiredFieldNumberValue("DurationMinutes", "Duration", interviewData.DurationMinutes, ModelState);
                validateInputs.ValidateNoNegativeNumber("DurationMinutes", "Duration", interviewData.DurationMinutes, ModelState);
                validateInputs.ValidateNumberLessOrEqualThanZero("DurationMinutes", "Duration", interviewData.DurationMinutes, ModelState);
                validateInputs.ValidateDateValidFormat("Date", "Action Date", interviewData.Date, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("Date", "Action Date", interviewData.Date, ModelState);

                if (ModelState.IsValid)
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var resultMessage = "";
                    var userActionedBy = claim.Value;

                    //IF IS NOT ID THEN CREATE IT
                    if (interviewData.InterviewId == null)
                    {
                        var res = await _unitOfWork.Interview.CreateInterview(userActionedBy, interviewData);

                        if (res.Success)
                        {
                            resultMessage = res.Message;
                        }
                        else
                        {
                            if (res.MessageType != "Validation Error")
                            {
                                return BadRequest(new { MessageType = res.MessageType, error = res.Message, result = "ErrorSaving", detail = $"The interview could not be saved." });
                            }
                            else
                            {
                                return BadRequest(new
                                {
                                    MessageType = res.MessageType,
                                    errors = new[] { res.Message }
                                });
                            }

                        }
                    }
                    else
                    {
                        //IF IS ID THEN UPDATE THE DEBIT/CREDIT
                        var res = await _unitOfWork.Interview.UpdateInterview(userActionedBy, interviewData);
                        if (res.Success)
                        {
                            resultMessage = res.Message;
                        }
                        else
                        {
                            if (res.MessageType != "Validation Error")
                            {
                                return BadRequest(new { error = res.Message, MessageType = res.MessageType, result = "ErrorSaving", detail = "The interview could not be updated." });
                            }
                            else
                            {
                                return BadRequest(new
                                {
                                    MessageType = res.MessageType,
                                    errors = new[] { res.Message }
                                });
                            }

                        }
                    }
                    return Ok(new
                    {
                        success = true,
                        message = resultMessage
                    });
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();
                    return BadRequest(new { MessageType = "Validation Error", message = "Validation Error", result = "error", errors = errors });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { MessageType = "Exception Error", error = $"There was an error saving the changes. More details: " + ex.Message, detail = ex.Message });
            }
        }

        [HttpGet("GetInterviewDataById")]
        public async Task<IActionResult> GetInterviewDataById(int interviewId)
        {
            try
            {
                var interviewData = await _unitOfWork.Interview.GetInterviewDataById(interviewId);
                if (interviewData == null)
                {
                    return BadRequest(new { error = "The interview is not longer in the database." });
                }

                return Ok(new
                {
                    interviewData = interviewData
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [HttpPost("RejectInterview")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectInterview([FromForm] int interviewId)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var res = await _unitOfWork.Interview.RejectInterview(claim.Value, interviewId);
                if (res.Success)
                {
                    return Ok(new { success = true, message = res.Message });
                }
                else
                {
                    return BadRequest(new { error = res.Message, MessageType = res.MessageType, result = "ErrorSaving", detail = "The interview could not be rejected." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error in the server, the interview could not be rejected.", result = "ErrorDeleting", detail = ex.Message });
            }
        }
    }
}
