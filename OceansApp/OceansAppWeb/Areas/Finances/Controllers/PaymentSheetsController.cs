using Azure.Storage.Queues;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;
using OceansApp.Models.ViewModels.AccountsPayable;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantPayments;
using OceansApp.Models.ViewModels.PaymentSheets;
using OceansApp.Models.ViewModels.ProjecConsultantPendingSubmission;
using OceansApp.Models.ViewModels.ProjectConsultantAssigned;
using OceansApp.Models.ViewModels.ReportingMyTimeSubmissions;
using OceansApp.Utility.NotificationTemplates;
using OceansApp.Utility.SharedMethods;
using OceansApp.Utility.SharedMethods.InputValidations;
using OceansAppWeb.Helpers;
using System.Globalization;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;

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
        private readonly IConfiguration _config;
        private readonly TelemetryClient _telemetryClient;
        private readonly Lazy<QueueClient> _queueClient;
        public PaymentSheetsController(IUnitOfWork unitOrWork, IConfiguration config,
            TelemetryClient telemetryClient, Lazy<QueueClient> queueClient)
        {
            _unitOfWork = unitOrWork;
            _config = config;
            _telemetryClient = telemetryClient;
            _queueClient = queueClient;
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ---- Manual Hours Upload (admin uploads hours on behalf of a consultant) ----
        // Surfaced as a modal on the Payment Sheets page (_ManualHoursUploadModalPartialView),
        // opened from a row's "Upload hours on behalf" button: the consultant, project and period
        // are taken from the row, so only the upload POST below is needed. Behind the existing
        // AccessToManageTheBasicsOfPaymentSheets policy (inherited at class level); no new
        // policy/claim. See docs/adr/0002.

        [HttpPost("UploadHoursOnBehalf")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadHoursOnBehalf([FromBody] UploadHoursOnBehalfVM model)
        {
            if (model == null)
            {
                return BadRequest(new { error = "The object data is null, it should be a valid object.", messageType = "Exception Error" });
            }
            ValidateInputs validateInputs = new();

            validateInputs.ValidateRequiredFieldIntType("ConsultantId", "Consultant", model.ConsultantId, ModelState);
            validateInputs.ValidateRequiredFieldIntType("ProjectId", "Project", model.ProjectId, ModelState);
            validateInputs.ValidateDateValidFormat("StartPeriodDate", "Start Period Date", model.StartPeriodDate, ModelState);
            validateInputs.ValidateRequiredFieldAnyValue("StartPeriodDate", "Start Period Date", model.StartPeriodDate, ModelState);
            validateInputs.ValidateDateValidFormat("EndPeriodDate", "End Period Date", model.EndPeriodDate, ModelState);
            validateInputs.ValidateRequiredFieldAnyValue("EndPeriodDate", "End Period Date", model.EndPeriodDate, ModelState);

            if (model.TotalHours <= 0)
            {
                ModelState.AddModelError("Hours", "Enter a total number of hours greater than zero.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(e => e.Value.Errors.Count > 0).ToDictionary(kvp => kvp.Key, kvp =>
                kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

                return BadRequest(new { errors = errors, messageType = "Validation Error" });
            }

            try
            {
                var userActionedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userActionedBy == null)
                {
                    return BadRequest(new { error = "User does not exist.", messageType = "Exception Error" });
                }

                MethodResponse response = await _unitOfWork.ReportingMyTimeMovement.UploadHoursOnBehalf(
                    userActionedBy, model.ConsultantId, model.ProjectId, model.StartPeriodDate, model.EndPeriodDate, model.TotalHours);

                if (!response.Success)
                {
                    if (response.MessageType == "Validation Error")
                    {
                        var errors = new Dictionary<string, List<string>>
                        {
                            { response.FieldName ?? "Hours", new List<string> { response.Message } }
                        };
                        return BadRequest(new { errors = errors, messageType = "Validation Error" });
                    }
                    return BadRequest(new { error = response.Message, messageType = response.MessageType });
                }

                // Fire the internal "new submission to review" email, just like a normal submission.
                // Resolve the consultant's name from the entity + user directly, NOT via
                // GetConsultantWithUserAsync — that VM projection hard-casts a nullable PaymentMethodId
                // and throws for consultants who have none set (e.g. internal/administrative), which would
                // silently drop the review email. Reading the entities can't hit that cast.
                // Still best-effort: the upload has already committed, so a notification/email-infra
                // failure here must NOT report the committed upload as failed. Log and carry on.
                try
                {
                    var subjectDetail = await _unitOfWork.ConsultantDetail
                        .GetFirstOrDefaultAsync(c => c.ConsultantId == model.ConsultantId);
                    var subjectUser = subjectDetail == null
                        ? null
                        : await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(u => u.Id == subjectDetail.UserId);
                    var consultantName = subjectUser == null
                        ? string.Empty
                        : $"{subjectUser.Name} {subjectUser.LastName}".Trim();
                    await SendNewSubmissionNotification(consultantName, model.StartPeriodDate, model.EndPeriodDate, model.ProjectId);
                }
                catch (Exception notifyEx)
                {
                    _telemetryClient.TrackException(notifyEx);
                }

                return Ok(new { success = true, message = response.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, messageType = "Exception Error" });
            }
        }

        // Read-only preview backing the modal's "Total hours for the period" default: returns the
        // server-computed workable-day count and the suggested total (N × 8). The modal calls this on
        // open to prefill the total and show "N days"; on failure it keeps submit disabled (no fallback
        // to a hardcoded 8). Behind the same AccessToManageTheBasicsOfPaymentSheets policy (inherited at
        // class level) as the upload. See docs/adr/0003.
        [HttpGet("PreviewWorkableDays")]
        public async Task<IActionResult> PreviewWorkableDays(int consultantId, int projectId, DateTime periodDate)
        {
            try
            {
                int workableDays = await _unitOfWork.ReportingMyTimeMovement
                    .GetWorkableDaysAsync(consultantId, projectId, periodDate);

                // The suggested default is a standard full-time day (8h) across every workable day. The
                // admin overrides for part-time/non-standard schedules; paid-holiday hours (which may use a
                // different per-assignment rate) are added separately by the payment computation, not here.
                const int standardFullTimeHoursPerDay = 8;
                return Ok(new { workableDays, suggestedTotal = workableDays * standardFullTimeHoursPerDay });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, messageType = "Exception Error" });
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task SendNewSubmissionNotification(string consultantName, DateTime startDate, DateTime endDate,
            int projectId)
        {
            string baseUrl = $"{HttpContext.Request.Scheme}://{Request.Host}/Finances/PaymentSheets";
            var project = await _unitOfWork.Project.GetFirstOrDefaultAsync(x => x.ProjectId == projectId);
            await SubmissionNotifications.SendNewSubmissionToReview(_queueClient, _config, _telemetryClient,
                baseUrl, consultantName, project?.Name ?? string.Empty, startDate, endDate);
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
                            validateInputs.ValidateNotRequiredAndStringLength("TransactionStatusName", "TransactionStatusName", jsonToValidate["Filters"]["TransactionStatusName"].ToString(), 80, ModelState);
                            validateInputs.ValidateNotRequiredAndStringLength("AccountsPayableStatusName", "AccountsPayableStatusName", jsonToValidate["Filters"]["AccountsPayableStatusName"].ToString(), 30, ModelState);
                            validateInputs.ValidateDateValidFormat("StartDate", "Start Date", jsonToValidate["Filters"]["StartDate"], ModelState);
                            validateInputs.ValidateDateValidFormat("EndDate", "End Date", jsonToValidate["Filters"]["EndDate"], ModelState);
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
                var userActionedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userActionedBy == null)
                {
                    return BadRequest(new { error = "User does not exist.", messageType = "Exception Error" });
                }

                string baseUrl = $"{HttpContext.Request.Scheme}://{Request.Host}";

                MethodResponse response = await _unitOfWork.ConsultantPayment.ApproveAndRejectSubmission(userActionedBy, dataFromUser, baseUrl);
                if (!response.Success)
                {
                    // Surface validation failures (e.g. consultant has no payment method) in the shape the
                    // approve modal renders as a warning toast, rather than the generic "unexpected error".
                    if (response.MessageType == "Validation Error")
                    {
                        var validationErrors = new Dictionary<string, List<string>>
                        {
                            { response.FieldName ?? "Submission", new List<string> { response.Message } }
                        };
                        return BadRequest(new { errors = validationErrors, messageType = "Validation Error" });
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
                var accountPayableList = await _unitOfWork.AccountPayable.GetAllAsync(x => x.ConsultantId == consultantId &&
                x.StartDatePeriod >= startDate && x.EndDatePeriod <= endDate && x.Voided == false);

                var closestToEndDateAP = accountPayableList
    .OrderBy(x => Math.Abs((x.EndDatePeriod - endDate).TotalDays))
    .FirstOrDefault();
                if (closestToEndDateAP != null)
                {
                    bool existsPayment = await _unitOfWork.ConsultantPayment.ExistsPaymentForAccountPayableAsync(closestToEndDateAP.AccountPayableId);
                    reportToSend.ExistsPayment = existsPayment;
                }

                decimal? balanceAmount = null;
                decimal? accountPayableAmount = null;

                if (accountPayableList.Count() > 0)
                {
                    balanceAmount = 0;
                    accountPayableAmount = 0;
                    foreach (var accountPayable in accountPayableList)
                    {
                        balanceAmount += accountPayable.BalanceAmount;
                        accountPayableAmount += accountPayable.Amount;
                    }
                }
                reportToSend.AccountPayableBalance = balanceAmount;
                reportToSend.AccountPayableAmount = accountPayableAmount;

                var movementsListFromDb = await _unitOfWork.ConsultantPayment.GetMovementsToPay(consultant, startDate, endDate);
                reportToSend.ListOfMovements = (GetListOfMovementsForPaymentVM?)movementsListFromDb.GenericObject;

                List<GetConsultantPaymentsInPeriodVM> paymentsList = await _unitOfWork.ConsultantPayment.GetConsultantPaymentsInPeriod(consultantId,
                startDate, endDate);
                reportToSend.PaymentsList = paymentsList;

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
        [HttpGet("GetAmountAndDetailsToMakePayment")]
        public async Task<IActionResult> GetAmountAndDetailsToMakePayment(int consultantId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var consultant = await _unitOfWork.ConsultantDetail.GetConsultantWithUserAsync(consultantId);
                if (consultant == null)
                {
                    return NotFound(new { error = "Consultant not found." });
                }

                MakePaymentVM reportToSend = new();
                reportToSend.ConsultantName = consultant.Name + " " + consultant.LastName;
                reportToSend.PaymentMethodId = consultant.PaymentMethodId;
                reportToSend.CountryName = consultant.CountryName;
                reportToSend.CompanyId = consultant.CompanyId;

                var accountPayableList = await _unitOfWork.AccountPayable.GetAllAsync(x => x.ConsultantId == consultant.ConsultantId &&
                x.StartDatePeriod >= startDate && x.EndDatePeriod <= endDate && x.Voided == false);

                decimal balanceAmount = 0;
                if (accountPayableList.Count() > 0)
                {
                    foreach (var accountPayable in accountPayableList)
                    {
                        balanceAmount += accountPayable.BalanceAmount;
                    }
                }

                decimal totalAmountToPay = 0;

                if (accountPayableList.Count() == 0)
                {
                    var movementsListFromDb = await _unitOfWork.ConsultantPayment.GetMovementsToPay(consultant, startDate, endDate);

                    totalAmountToPay = _unitOfWork.ConsultantPayment.GetConsultantTotalAmountToPay((GetListOfMovementsForPaymentVM?)movementsListFromDb.GenericObject);
                }
                else
                {
                    totalAmountToPay = balanceAmount;
                }

                reportToSend.AmountToPay = totalAmountToPay;

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

        [HttpGet("GetPaymentDataByPaymentId")]
        public async Task<IActionResult> GetPaymentDataByPaymentId(int paymentId)
        {
            try
            {
                var existingPayment = await _unitOfWork.ConsultantPayment.GetFirstOrDefaultAsync(x => x.ConsultantPaymentId == paymentId);
                if (existingPayment == null)
                {
                    return NotFound(new { error = "The payment no longer exists." });
                }

                var consultant = await _unitOfWork.ConsultantDetail.GetConsultantWithUserAsync(existingPayment.ConsultantId);
                if (consultant == null)
                {
                    return NotFound(new { error = "Consultant not found." });
                }

                MakePaymentVM reportToSend = new();
                reportToSend.ConsultantName = consultant.Name + " " + consultant.LastName;
                reportToSend.PaymentMethodId = existingPayment.PaymentMethodId;
                reportToSend.CountryName = consultant.CountryName;
                reportToSend.CompanyId = existingPayment.CompanyId;
                reportToSend.AmountToPay = existingPayment.PaymentAmount;
                reportToSend.AccountingDate = existingPayment.AccountingDate;
                reportToSend.ReferenceNumber = existingPayment.ReferenceNumber;

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

        [HttpPost("CreateUpdatePayment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdatePayment([FromBody] CreateUpdateConsultantPaymentVM paymentData)
        {
            try
            {
                if (paymentData == null)
                {
                    return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
                }
                ValidateInputs validateInputs = new();

                validateInputs.ValidateRequiredFieldIntType("ConsultantId", "ConsultantId", paymentData.ConsultantId, ModelState);
                validateInputs.ValidateDateValidFormat("StartDatePeriod", "Start Date Period", paymentData.StartDatePeriod, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("StartDatePeriod", "Start Date Period", paymentData.StartDatePeriod, ModelState);
                validateInputs.ValidateDateValidFormat("EndDatePeriod", "End Date Period", paymentData.EndDatePeriod, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("EndDatePeriod", "End Date Period", paymentData.EndDatePeriod, ModelState);
                validateInputs.ValidateDateValidFormat("AccountingDate", "Accounting Date", paymentData.AccountingDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("AccountingDate", "Accounting Date", paymentData.AccountingDate, ModelState);
                validateInputs.ValidateNoNegativeNumber("PaymentAmount", "Amount to pay", paymentData.PaymentAmount, ModelState);
                validateInputs.ValidateNumberLessOrEqualThanZero("PaymentAmount", "Amount to pay", paymentData.PaymentAmount, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("PaymentAmount", "Amount to pay", paymentData.PaymentAmount, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("CompanyId", "CompanyId", paymentData.CompanyId, ModelState);
                validateInputs.ValidateRequiredAndStringLength("CompanyId", "CompanyId", paymentData.CompanyId, 8, ModelState);
                validateInputs.ValidateRequiredFieldIntType("BankAccountId", "Bank Account", paymentData.BankAccountId, ModelState);
                validateInputs.ValidateRequiredAndStringLength("ReferenceNumber", "Reference Number", paymentData.ReferenceNumber, 50, ModelState);

                if (ModelState.IsValid)
                {
                    string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var resultMessage = "";

                    //IF IS NOT PAYMENT ID THEN CREATE THE PAYMENT
                    if (paymentData.ConsultantPaymentId == null)
                    {
                        var consultant = await _unitOfWork.ConsultantDetail.GetConsultantWithUserAsync((int)paymentData.ConsultantId);
                        if (consultant == null)
                        {
                            return NotFound(new { error = "Consultant not found." });
                        }
                        var movementsListFromDb = await _unitOfWork.ConsultantPayment.GetMovementsToPay(consultant, DateTime.Parse(paymentData.StartDatePeriod),
                            DateTime.Parse(paymentData.EndDatePeriod));
                        decimal totalAmountToPay = 0;

                        totalAmountToPay = _unitOfWork.ConsultantPayment.GetConsultantTotalAmountToPay((GetListOfMovementsForPaymentVM?)movementsListFromDb.GenericObject);

                        var res = await _unitOfWork.ConsultantPayment.CreatePayment(userActionedBy, paymentData, totalAmountToPay, (GetListOfMovementsForPaymentVM)movementsListFromDb.GenericObject);

                        if (res.Success)
                        {
                            resultMessage = res.Message;
                        }
                        else
                        {
                            if (res.MessageType == "Validation Error")
                            {
                                return BadRequest(new { MessageType = "Validation Error", errors = new[] { res.Message } });
                            }
                            return BadRequest(new { MessageType = res.MessageType, error = res.Message });
                        }
                    }
                    else
                    {
                        //IF IS PAYMENT ID THEN UPDATE THE PAYMENT
                        var res = await _unitOfWork.ConsultantPayment.UpdatePayment(userActionedBy, paymentData);
                        if (res.Success)
                        {
                            resultMessage = res.Message;
                        }
                        else
                        {
                            if (res.MessageType == "Validation Error")
                            {
                                return BadRequest(new { MessageType = "Validation Error", errors = new[] { res.Message } });
                            }
                            return BadRequest(new { error = res.Message, MessageType = res.MessageType });
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

        [HttpPost("SetAsAccountPayable")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAsAccountPayable([FromBody] SetAsAccountPayableVM dataFromModel)
        {
            try
            {
                if (dataFromModel == null)
                {
                    return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
                }
                ValidateInputs validateInputs = new();

                validateInputs.ValidateRequiredFieldIntType("ConsultantId", "ConsultantId", dataFromModel.ConsultantId, ModelState);
                validateInputs.ValidateDateValidFormat("StartDatePeriod", "Start Date Period", dataFromModel.StartDatePeriod, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("StartDatePeriod", "Start Date Period", dataFromModel.StartDatePeriod, ModelState);
                validateInputs.ValidateDateValidFormat("EndDatePeriod", "End Date Period", dataFromModel.EndDatePeriod, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("EndDatePeriod", "End Date Period", dataFromModel.EndDatePeriod, ModelState);

                if (ModelState.IsValid)
                {
                    string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var resultMessage = "";

                    var consultant = await _unitOfWork.ConsultantDetail.GetConsultantWithUserAsync((int)dataFromModel.ConsultantId);
                    if (consultant == null)
                    {
                        return NotFound(new { error = "Consultant not found." });
                    }
                    var movementsListFromDb = await _unitOfWork.ConsultantPayment.GetMovementsToPay(consultant, DateTime.Parse(dataFromModel.StartDatePeriod),
                        DateTime.Parse(dataFromModel.EndDatePeriod));
                    decimal totalAmountToPay = 0;

                    totalAmountToPay = _unitOfWork.ConsultantPayment.GetConsultantTotalAmountToPay((GetListOfMovementsForPaymentVM?)movementsListFromDb.GenericObject);

                    var res = await _unitOfWork.ConsultantPayment.SetAsAccountPayable(userActionedBy, dataFromModel, totalAmountToPay,
                        (GetListOfMovementsForPaymentVM)movementsListFromDb.GenericObject, consultant.CompanyId);

                    if (res.Success)
                    {
                        resultMessage = res.Message;
                    }
                    else
                    {
                        if (res.MessageType == "Validation Error")
                        {
                            return BadRequest(new { MessageType = "Validation Error", errors = new[] { res.Message } });
                        }
                        return BadRequest(new { MessageType = res.MessageType, error = res.Message });
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

        [HttpDelete("DeletePayment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePayment(int paymentId)
        {
            try
            {
                MethodResponse res = await _unitOfWork.ConsultantPayment.DeletePayment(paymentId);

                if (res.Success)
                {
                    return Ok(new { success = true, message = res.Message });
                }
                else
                {
                    return BadRequest(new { MessageType = res.MessageType, error = res.Message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error in the server, the payment could not be deleted.", detail = ex.Message });
            }
        }

        [HttpPost("RemoveProjectConsultantInPeriod")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProjectConsultantInPeriod([FromBody] RemoveProjectConsultantInPeriodVM model)
        {
            if (model == null)
            {
                return BadRequest(new { error = "The object data is null, it should be a valid object.", messageType = "Exception Error" });
            }
            ValidateInputs validateInputs = new();

            validateInputs.ValidateRequiredFieldIntType("ProjectId", "ProjectId", model.ProjectId, ModelState);
            validateInputs.ValidateRequiredFieldIntType("ConsultantId", "ConsultantId", model.ConsultantId, ModelState);
            validateInputs.ValidateDateValidFormat("StartDatePeriod", "Start Date Period", model.StartPeriodDate, ModelState);
            validateInputs.ValidateRequiredFieldAnyValue("StartDatePeriod", "Start Date Period", model.StartPeriodDate, ModelState);
            validateInputs.ValidateDateValidFormat("EndDatePeriod", "End Date Period", model.EndPeriodDate, ModelState);
            validateInputs.ValidateRequiredFieldAnyValue("EndtDatePeriod", "End Date Period", model.EndPeriodDate, ModelState);

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
                ProjectConsultantPeriodDisabledTracking disableTracking = new()
                {
                    ProjectId = model.ProjectId.Value,
                    ConsultantId = model.ConsultantId,
                    StartPeriodDate = model.StartPeriodDate,
                    EndPeriodDate = model.EndPeriodDate,
                    CreationDate = DateTime.UtcNow,
                    CreatedBy = userActionedBy
                };
                await _unitOfWork.ProjectConsultantPeriodDisabledTracking.AddAsync(disableTracking);

                var projectMovements = await _unitOfWork.ReportingMyTimeMovement.GetAllAsync(x => x.ProjectId == model.ProjectId.Value &&
                  x.ConsultantId == model.ConsultantId && (x.ActionDate.Date >= model.StartPeriodDate.Date &&
                  x.ActionDate.Date <= model.EndPeriodDate.Date));

                foreach (var movement in projectMovements)
                {
                    _unitOfWork.ReportingMyTimeMovement.Remove(movement);
                }

                var transactionStatus = await _unitOfWork.TransactionStatus.GetFirstOrDefaultAsync(x => x.Name == "Approved");
                if (transactionStatus == null)
                {
                    return BadRequest(new { error = "Transaction status 'Approved' not found.", messageType = "Exception Error" });
                }
                var movementType = await _unitOfWork.ReportingMyTimeMovementType.GetFirstOrDefaultAsync(x => x.Name == "Normal Hours");
                if (movementType == null)
                {
                    return BadRequest(new { error = "Movement Type 'Normal Hours' not found.", messageType = "Exception Error" });
                }
                var timeMovementToCreate = new ReportingMyTimeMovement
                {
                    ConsultantId = model.ConsultantId,
                    ProjectId = model.ProjectId.Value,
                    ActionDate = model.EndPeriodDate,
                    Notes = "No hours to report for this period.",
                    TransactionStatusId = transactionStatus.TransactionStatusId,
                    MovementTypeId = movementType.MovementTypeId,
                    CreationDate = DateTime.UtcNow,
                    Quantity = 0
                };
                await _unitOfWork.ReportingMyTimeMovement.AddAsync(timeMovementToCreate);

                var submission = await _unitOfWork.ReportingMyTimeMovementSubmission.GetFirstOrDefaultAsync(x => x.ProjectId == model.ProjectId.Value &&
                 x.ConsultantId == model.ConsultantId && (x.StartPeriodDate.Date == model.StartPeriodDate.Date &&
                 x.EndPeriodDate.Date == model.EndPeriodDate.Date));

                if (submission != null)
                {
                    submission.TransactionStatusId = transactionStatus.TransactionStatusId;
                }
                else
                {
                    //Create submission
                    var submissionToCreate = new ReportingMyTimeMovementSubmission
                    {
                        ConsultantId = model.ConsultantId,
                        ProjectId = model.ProjectId.Value,
                        TransactionStatusId = transactionStatus.TransactionStatusId,
                        SubmissionDate = DateTime.UtcNow,
                        StartPeriodDate = model.StartPeriodDate,
                        EndPeriodDate = model.EndPeriodDate
                    };

                    await _unitOfWork.ReportingMyTimeMovementSubmission.AddAsync(submissionToCreate);
                }

                await _unitOfWork.SaveAsync();

                return Ok(new { success = true, message = "The project was removed for this period" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, messageType = "Exception Error" });
            }
        }

        [HttpGet("GetRemovedProjectsInPeriod")]
        public async Task<IActionResult> GetRemovedProjectsInPeriod(DateTime startDate, DateTime endDate)
        {
            try
            {
                var removedProjectsList = await _unitOfWork.ProjectConsultantPeriodDisabledTracking.GetRemovedProjectsInPeriodAsync(startDate, endDate);

                return Ok(new
                {
                    removedProjectsList = removedProjectsList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [HttpPost("AddProjectInPeriod")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProjectInPeriod([FromForm] int id)
        {
            try
            {
                var removedProject = await _unitOfWork.ProjectConsultantPeriodDisabledTracking.GetFirstOrDefaultAsync(x => x.Id == id);
                if (removedProject == null)
                {
                    return BadRequest(new { error = "The removed project was not found" });
                }
                var accountsPayable = await _unitOfWork.AccountPayable
    .GetFirstOrDefaultAsync(
        x => x.ConsultantId == removedProject.ConsultantId &&
             x.StartDatePeriod == removedProject.StartPeriodDate &&
             x.EndDatePeriod == removedProject.EndPeriodDate);
                _unitOfWork.ProjectConsultantPeriodDisabledTracking.Remove(removedProject);

                var transactionStatus = await _unitOfWork.TransactionStatus.GetFirstOrDefaultAsync(x => x.Name == "Rejected");
                if (transactionStatus == null)
                {
                    return BadRequest(new { error = "Transaction status 'Rejected' not found.", messageType = "Exception Error" });
                }

                var projectMovements = await _unitOfWork.ReportingMyTimeMovement.GetAllAsync(x => x.ProjectId == removedProject.ProjectId &&
                  x.ConsultantId == removedProject.ConsultantId && (x.ActionDate.Date >= removedProject.StartPeriodDate.Date &&
                  x.ActionDate.Date <= removedProject.EndPeriodDate.Date));

                foreach (var movement in projectMovements)
                {
                    _unitOfWork.ReportingMyTimeMovement.Remove(movement);
                }

                var submission = await _unitOfWork.ReportingMyTimeMovementSubmission.GetFirstOrDefaultAsync(x => x.ProjectId == removedProject.ProjectId &&
                  x.ConsultantId == removedProject.ConsultantId && (x.StartPeriodDate.Date == removedProject.StartPeriodDate.Date &&
                  x.EndPeriodDate.Date == removedProject.EndPeriodDate.Date));

                if (submission != null)
                {
                    submission.TransactionStatusId = transactionStatus.TransactionStatusId;
                }

                await _unitOfWork.SaveAsync();

                return Ok(new { success = true, message = "The project was added successfully to the period." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error in the server, the project could not be added.", result = "ErrorDeleting", detail = ex.Message });
            }
        }

        [HttpPost("FixDifferenceToMakePaymentToday")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FixDifferenceToMakePaymentToday([FromBody] SetAsAccountPayableVM dataFromModel)
        {
            try
            {
                if (dataFromModel == null)
                {
                    return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
                }
                ValidateInputs validateInputs = new();

                validateInputs.ValidateRequiredFieldIntType("ConsultantId", "ConsultantId", dataFromModel.ConsultantId, ModelState);
                validateInputs.ValidateDateValidFormat("StartDatePeriod", "Start Date Period", dataFromModel.StartDatePeriod, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("StartDatePeriod", "Start Date Period", dataFromModel.StartDatePeriod, ModelState);
                validateInputs.ValidateDateValidFormat("EndDatePeriod", "End Date Period", dataFromModel.EndDatePeriod, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("EndDatePeriod", "End Date Period", dataFromModel.EndDatePeriod, ModelState);

                if (ModelState.IsValid)
                {
                    string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    var res = await _unitOfWork.ConsultantPayment.FixDifferenceToMayPaymentAsync((int)dataFromModel.ConsultantId,
                        DateTime.Parse(dataFromModel.StartDatePeriod), DateTime.Parse(dataFromModel.EndDatePeriod),
            userActionedBy);

                    if (res.Success)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = res.Message
                        });
                    }
                    else
                    {
                        if (res.MessageType == "Validation Error")
                        {
                            return BadRequest(new { MessageType = "Validation Error", errors = new[] { res.Message } });
                        }
                        return BadRequest(new { MessageType = res.MessageType, error = res.Message });
                    }
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

        [HttpGet("GetDifferenceToDeferNextPeriod")]
        public async Task<IActionResult> GetDifferenceToDeferNextPeriod(int consultantId, DateTime startDate, DateTime endDate)
        {
            try
            {
                ValidateInputs validateInputs = new();

                validateInputs.ValidateRequiredFieldIntType("ConsultantId", "ConsultantId", consultantId, ModelState);
                validateInputs.ValidateDateValidFormat("StartDatePeriod", "Start Date Period", startDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("StartDatePeriod", "Start Date Period", startDate, ModelState);
                validateInputs.ValidateDateValidFormat("EndDatePeriod", "End Date Period", endDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("EndDatePeriod", "End Date Period", endDate, ModelState);

                if (ModelState.IsValid)
                {
                    string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    var dataToReturn = await _unitOfWork.ConsultantPayment.GetMovementsToDeferAsync(consultantId,
                        startDate, endDate);

                    return Ok(new
                    {
                        data = dataToReturn
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

        [HttpPost("DeferDebitCredit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeferDebitCredit([FromBody] DeferDebitCreditVM modelData)
        {
            try
            {
                if (modelData == null)
                {
                    return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
                }
                ValidateInputs validateInputs = new();

                validateInputs.ValidateRequiredFieldIntType("ConsultantId", "Consultant Id", modelData.ConsultantId, ModelState);
                validateInputs.ValidateDateValidFormat("ActionDate", "Action Date", modelData.ActionDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("ActionDate", "Action Date", modelData.ActionDate, ModelState);
                validateInputs.ValidateDateValidFormat("StartDatePeriod", "Start Date Period", modelData.StartDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("StartDatePeriod", "Start Date Period", modelData.StartDate, ModelState);
                validateInputs.ValidateDateValidFormat("EndDatePeriod", "End Date Period", modelData.EndDate, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("EndDatePeriod", "End Date Period", modelData.EndDate, ModelState);

                if (modelData.MovementsList == null)
                {
                    ModelState.AddModelError("Movements", "The movements are required.");
                }
                else
                {
                    foreach (var movement in modelData.MovementsList)
                    {
                        validateInputs.ValidateRequiredFieldIntType("MovementId", "Movement Id", movement.Id, ModelState);
                        validateInputs.ValidateRequiredFieldIntType("AccountingAccountId", "Accounting Account", movement.AccountingAccountId, ModelState);
                        validateInputs.ValidateRequiredFieldIntType("CostCenterId", "Cost Center", movement.CostCenterId, ModelState);
                        validateInputs.ValidateRequiredFieldStringValue("Description", "Description", movement.Description, ModelState);
                        validateInputs.ValidateRequiredAndStringLength("Description", "Description", movement.Description, 100, ModelState);
                    }
                }

                if (ModelState.IsValid)
                {
                    string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userActionedBy == null)
                    {
                        return BadRequest(new { error = "User does not exist.", messageType = "Exception Error" });
                    }

                    var res = await _unitOfWork.ConsultantPaymentsDebitsCredits
                        .CreateDebitCreditWithListAsync(userActionedBy, modelData);

                    if (res.Success)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = res.Message
                        });
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

        [HttpPost("SendSubmissionReminders")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendSubmissionReminders([FromBody] SendPendingSubmissionReminderVM model)
        {
            if (model == null)
            {
                return BadRequest(new { error = "The object data is null, it should be a valid object.", messageType = "Exception Error" });
            }
            ValidateInputs validateInputs = new();

            validateInputs.ValidateDateValidFormat("StartDate", "Start Date", model.StartDate, ModelState);
            validateInputs.ValidateRequiredFieldAnyValue("StartDate", "Start Date", model.StartDate, ModelState);
            validateInputs.ValidateDateValidFormat("EndDatePeriod", "End Date Period", model.EndDate, ModelState);
            validateInputs.ValidateRequiredFieldAnyValue("EndtDatePeriod", "End Date Period", model.EndDate, ModelState);
            validateInputs.ValidateRequiredFieldIntType("PaymentPeriod", "PaymentPeriod", model.PaymentPeriod, ModelState);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(e => e.Value.Errors.Count > 0).ToDictionary(kvp => kvp.Key, kvp =>
                kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

                return BadRequest(new { errors = errors, messageType = "Validation Error" });
            }

            try
            {
                var saveRegistersResponse = await _unitOfWork.ProjectConsultantPendingSubmission
                    .CreateProjectsConsultantsPendingSubmissionsAsync(
                    (DateTime)model.StartDate, (DateTime)model.EndDate, (int)model.PaymentPeriod
                    );
                string successMessage = "";

                if (saveRegistersResponse.Success)
                {
                    List<ConsultantAndProjectVM> projectConsultantsList = (List<ConsultantAndProjectVM>)saveRegistersResponse.GenericObject;
                    //Send emails
                    var groupedConsultants = projectConsultantsList
                        .GroupBy(c => new { c.ConsultantId, c.ConsultantName, c.Email })
                        .Select(group => new GetConsultantDataVM
                        {
                            ConsultantId = group.Key.ConsultantId,
                            ConsultantName = group.Key.ConsultantName,
                            Email = group.Key.Email,
                            Projects = group.Select(p => new GetProjectNamesVM
                            {
                                ProjectName = p.ProjectName
                            }).ToList()
                        }).ToList();
                    if (groupedConsultants.Count == 0)
                    {
                        successMessage = $"No consultants are with pending submissions.";
                    }
                    else
                    {
                        DateTime startDateTime = (DateTime)model.StartDate;
                        DateTime endDateTime = (DateTime)model.EndDate;
                        string startDateFormated = startDateTime.ToString("MMM d", System.Globalization.CultureInfo.InvariantCulture);
                        string endDateFormated = endDateTime.ToString("MMM d", System.Globalization.CultureInfo.InvariantCulture);

                        string startDateFormattedYYYYMMDD = startDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        string endDateFormattedYYYYMMDD = endDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);


                        string periodString = $"{startDateFormated} - {endDateFormated}";

                        //Send emails
                        foreach (var projectConsultantData in groupedConsultants)
                        {
                            var emailToSend = PrepareEmailContentSubmissionReminder(projectConsultantData.ConsultantName,
                                projectConsultantData.Email, projectConsultantData.Projects, periodString, startDateFormattedYYYYMMDD, endDateFormattedYYYYMMDD);
                            try
                            {
                                _telemetryClient.TrackTrace($"Sending email to {projectConsultantData.Email}");
                                string message = JsonConvert.SerializeObject(emailToSend);
                                var testing = await _queueClient.Value.SendMessageAsync(StringsMethods.Base64Encode(message));

                                _telemetryClient.TrackTrace($"Email sent to {projectConsultantData.Email}");
                            }
                            catch (Exception ex)
                            {
                                _telemetryClient.TrackException(ex);
                                _telemetryClient.TrackTrace($"Failed sending Email to {projectConsultantData.Email}!");
                                continue;
                            }
                        }

                        successMessage = $"The reminder was sent to {groupedConsultants.Count} consultant{(groupedConsultants.Count > 1 ? "s" : "")} that {(groupedConsultants.Count > 1 ? "are" : "is")} pending submissions!";
                    }
                }
                else
                {
                    return BadRequest(new { error = saveRegistersResponse.Message, messageType = "Exception Error" });
                }
                return Ok(new { success = true, message = successMessage });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, messageType = "Exception Error" });
            }
        }

        [HttpPost("EditHoursFromApprovals")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHoursFromApprovals([FromBody] List<EditHoursFromPaymentSheetsVM> model)
        {
            if (model == null)
            {
                return BadRequest(new { error = "The object data is null, it should be a valid object.", messageType = "Exception Error" });
            }
            ValidateInputs validateInputs = new();

            foreach (var item in model)
            {
                validateInputs.ValidateRequiredFieldAnyValue("MovementId", "MovementId", item.MovementId, ModelState);

                if (item.TimeFrom == null && item.TimeTo == null)
                {
                    validateInputs.ValidateRequiredFieldAnyValue("Quantity", "Quantity", item.Quantity, ModelState);
                }

                if (item.Quantity == null)
                {
                    validateInputs.ValidateRequiredFieldAnyValue("TimeFrom", "Time From", item.TimeFrom, ModelState);
                    validateInputs.ValidateRequiredFieldAnyValue("TimeTo", "Time To", item.TimeTo, ModelState);
                    validateInputs.ValidateRequiredFieldAnyValue("Remove", "Remove", item.Remove, ModelState);
                }
            }

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

                var response = await _unitOfWork.ReportingMyTimeMovement.UpdateTimeFromPaymentSheets(userActionedBy, model);

                if (response.Success)
                {
                    return Ok(new { success = true, message = response.Message });
                }
                else
                {
                    return BadRequest(new
                    {
                        MessageType = response.MessageType,
                        errors = new[] { response.Message }
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, messageType = "Exception Error" });
            }
        }

        //NO HTTP METHODS
        [ApiExplorerSettings(IgnoreApi = true)]
        private SendEmailVM PrepareEmailContentSubmissionReminder(string consultantName, string consultantEmail, List<GetProjectNamesVM> projects, string period, string startDateFormatted, 
            string endDateFormatted)
        {
            string baseUrl = $"{HttpContext.Request.Scheme}://{Request.Host}/TrackingTool/ReportingMyTime?startDate={startDateFormatted}&endDate={endDateFormatted}";
            List<string> projectsStringList = new();
            foreach (var project in projects)
            {
                projectsStringList.Add(project.ProjectName);
            }
            var emailTemplates = new EmailTemplates();
            var emailBody = emailTemplates.SubmissionReminderBody(baseUrl, consultantName.Trim(), period, projectsStringList);
            var templateEmail = emailTemplates.EmailTemplate("SUBMIT HOURS FOR PAYMENT", emailBody);

            return new SendEmailVM
            {
                Subject = "YOU HAVE PENDING SUBMISSIONS FOR PAYMENT - RIPPLE BY OCEANS",
                SharedEmailFrom = _config["SharedMailboxEmailRippleApp"],
                EmailTo = consultantEmail.Trim(),
                Body = templateEmail
            };
        }


    }
}
