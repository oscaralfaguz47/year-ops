using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.PaymentSheets;
using OceansApp.Utility.SharedMethods;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Security.Claims;

namespace OceansAppWeb.Areas.Finances.Controllers
{
    [ApiController]
    [Route("Finances/[controller]")]
    [Area("Finances")]
    [Authorize]
    [Authorize(Policy = "AccessToManageTheBasicsOfPaymentSheets")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    public class PaymentSheetsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public PaymentSheetsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetConsultantsToPayList")]
        public async Task<IActionResult> GetConsultantsToPayList(string model)
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
                            validateInputs.ValidateNonRequiredFieldIntType("ProjectId", "Project", (int?)jsonToValidate["Filters"]["ProjectId"], ModelState);
                            validateInputs.ValidateNonRequiredFieldIntType("PaymentPeriod", "Payment Period", (int?)jsonToValidate["Filters"]["PaymentPeriod"], ModelState);

                            if (!ModelState.IsValid)
                            {
                                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                              .Select(e => e.ErrorMessage)
                                                              .ToList();
                                return BadRequest(new { MessageType = "Validation Error", message = "Validation Error", result = "error", errors = errors });
                            }
                        }
                    }
                }

                PaymentSheetsPaginationFiltersVM paymentSheetsPaginationFilters = System.Text.Json.JsonSerializer.Deserialize<PaymentSheetsPaginationFiltersVM>(model);

                PaymentSheetsPaginationFiltersVM paginationFilters = new();
                paginationFilters.Filters = new PaymentSheetsFiltersGetAllVM();

                int numAppliedFilters = 0;
                if (paymentSheetsPaginationFilters.Filters != null)
                {
                    foreach (var prop in paymentSheetsPaginationFilters.Filters.GetType().GetProperties())
                    {
                        var value = prop.GetValue(paymentSheetsPaginationFilters.Filters, null);
                        if (value is not null and not "")
                        {
                            numAppliedFilters++;
                        }
                    }
                }
                var setPagination = new PaginationFiltersBehavior();
                paginationFilters.PaginationWithoutFilters = setPagination.SetPagination(paymentSheetsPaginationFilters.PaginationWithoutFilters, numAppliedFilters);

                if (numAppliedFilters > 0)
                {
                    paginationFilters.Filters = paymentSheetsPaginationFilters.Filters;
                }

                var totalResults = await _unitOfWork.ConsultantDetail.GetAllConsultantsToPayWithFiltersAsync(paginationFilters);
                paginationFilters.PaginationWithoutFilters.Pagination.TotalResults = totalResults.totalCount;

                var data = new { consultantsToPayList = totalResults.consultantsToPay, PaginationFilters = paginationFilters };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"There was an error fetching the list of consultants." }, success = false, result = "errorGet", detail = ex.Message });
            }
        }

        [HttpGet("GetReportDetailsFromSubmissionById")]
        public async Task<IActionResult> GetReportDetailsFromSubmissionById(int submissionId)
        {
            try
            {
                var reportDetails = await _unitOfWork.ConsultantDetail.GetReportDetailsFromSubmission(submissionId);
                if (reportDetails == null)
                {
                    return BadRequest(new { error = "The submission is not longer in the database." });
                }

                return Ok(new
                {
                    reportDetails = reportDetails
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [HttpPost("RejectApproveSubmission")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectApproveSubmission([FromBody] ApproveRejectSubmissionVM dataFromUser)
        {
            if (dataFromUser == null)
            {
                return BadRequest(new { error = "The object data is null, it should be a valid object.", messageType = "Exception Error" });
            }
            ValidateInputs validateInputs = new();

            validateInputs.ValidateRequiredFieldIntType("SubmissionId", "SubmissionId", dataFromUser.SubmissionId, ModelState);
            validateInputs.ValidateRequiredFieldStringValue("Action", "Action", dataFromUser.TransactionStatus, ModelState);
            if (dataFromUser.TransactionStatus == "Rejected") validateInputs.ValidateRequiredFieldStringValue("Body", "Message", dataFromUser.Body, ModelState);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(e => e.Value.Errors.Count > 0).ToDictionary(kvp => kvp.Key, kvp =>
                kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

                return BadRequest(new { errors = errors, messageType = "Validation Error" });
            }

            try
            {
                string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userActionedBy == null)
                {
                    return BadRequest(new { error = "User does not exist.", messageType = "Exception Error" });
                }

                MethodResponse response = await _unitOfWork.ConsultantDetail.ApproveAndRejectSubmission(userActionedBy, dataFromUser);
                if (!response.Success)
                {
                    return BadRequest(new { error = response.Message, messageType = response.MessageType });
                }
                return Ok(new { success = true, message = response.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, messageType = "Exception Error" });
            }
        }

        [HttpGet("GetReportToMakePayment")]
        public async Task<IActionResult> GetReportToMakePayment(int consultantId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var consultant = await _unitOfWork.ConsultantDetail.GetConsultantWithUserAsync(consultantId);
                if (consultant == null)
                {
                    return NotFound(new { error = "Consultant not found." });
                }

                GetReportToMakePaymentVM reportToSend = new();
                reportToSend.ConsultantName = consultant.Name + " " + consultant.LastName;
                reportToSend.PaymentMethodId = consultant.PaymentMethodId;
                reportToSend.CountryId = consultant.CountryId;
                reportToSend.CompanyId = consultant.CompanyId;
                var movementsListFromDb = await _unitOfWork.ConsultantPayment.GetMovementsToPay(consultant, startDate, endDate);
                reportToSend.ListOfMovements = (GetListOfMovementsForPaymentVM?)movementsListFromDb.GenericList;

                return Ok(new
                {
                    reportDetails = reportToSend
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
    }
}
