using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.AccountsPayable;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantPayments;
using OceansApp.Models.ViewModels.PaymentSheets;
using OceansApp.Models.ViewModels.ProjectConsultantAssigned;
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
                            validateInputs.ValidateNotRequiredAndStringLength("TransactionStatusName", "TransactionStatusName", jsonToValidate["Filters"]["TransactionStatusName"].ToString(), 80, ModelState);
                            validateInputs.ValidateNotRequiredAndStringLength("AccountsPayableStatusName", "AccountsPayableStatusName", jsonToValidate["Filters"]["AccountsPayableStatusName"].ToString(), 20, ModelState);
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
                var accountPayableList = await _unitOfWork.AccountPayable.GetAllAsync(x => x.ConsultantId == consultantId &&
                x.StartDatePeriod >= startDate && x.EndDatePeriod <= endDate);
                decimal? balanceAmount = null;
                if (accountPayableList.Count() > 0)
                {
                    balanceAmount = 0;
                    foreach (var accountPayable in accountPayableList)
                    {
                        balanceAmount += accountPayable.BalanceAmount;
                    }
                }
                reportToSend.AccountPayableBalance = balanceAmount;

                var movementsListFromDb = await _unitOfWork.ConsultantPayment.GetMovementsToPay(consultant, startDate, endDate);
                reportToSend.ListOfMovements = (GetListOfMovementsForPaymentVM?)movementsListFromDb.GenericList;

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
                x.StartDatePeriod >= startDate && x.EndDatePeriod <= endDate);

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

                    totalAmountToPay = _unitOfWork.ConsultantPayment.GetConsultantTotalAmountToPay((GetListOfMovementsForPaymentVM?)movementsListFromDb.GenericList);
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

                        totalAmountToPay = _unitOfWork.ConsultantPayment.GetConsultantTotalAmountToPay((GetListOfMovementsForPaymentVM?)movementsListFromDb.GenericList);

                        var res = await _unitOfWork.ConsultantPayment.CreatePayment(userActionedBy, paymentData, totalAmountToPay, (GetListOfMovementsForPaymentVM)movementsListFromDb.GenericList);

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

                    totalAmountToPay = _unitOfWork.ConsultantPayment.GetConsultantTotalAmountToPay((GetListOfMovementsForPaymentVM?)movementsListFromDb.GenericList);

                    var res = await _unitOfWork.ConsultantPayment.SetAsAccountPayable(userActionedBy, dataFromModel, totalAmountToPay,
                        (GetListOfMovementsForPaymentVM)movementsListFromDb.GenericList, consultant.CompanyId);

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
                    ProjectId = model.ProjectId,
                    ConsultantId = model.ConsultantId,
                    StartPeriodDate = model.StartPeriodDate,
                    EndPeriodDate = model.EndPeriodDate,
                    CreationDate = DateTime.UtcNow,
                    CreatedBy = userActionedBy
                };
                await _unitOfWork.ProjectConsultantPeriodDisabledTracking.AddAsync(disableTracking);
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
                await _unitOfWork.SaveAsync();

                return Ok(new { success = true, message = "The project was added successfully to the period." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error in the server, the project could not be added.", result = "ErrorDeleting", detail = ex.Message });
            }
        }
    }
}
