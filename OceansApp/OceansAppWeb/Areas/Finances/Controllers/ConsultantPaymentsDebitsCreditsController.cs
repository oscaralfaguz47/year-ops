using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantPaymentsDebitsCredits;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Security.Claims;

namespace OceansAppWeb.Areas.Finances.Controllers
{
    [Area("Finances")]
    [Authorize]
    [Authorize(Policy = "AccessToManageConsultantPaymentsDebitsAndCredits")]
    [RequireTwoFactorEnabled]
    public class ConsultantPaymentsDebitsCreditsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        public ConsultantPaymentsDebitsCreditsController(IUnitOfWork unitOrWork, IConfiguration config)
        {
            _unitOfWork = unitOrWork;
            _config = config;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetConsultantPaymentsDebitsCreditsList(string model)
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

                ConsultantPaymentsDebitsCreditsPaginationFiltersVM consultantPaymentsDebitsCreditsPaginationFilters = System.Text.Json.JsonSerializer.Deserialize<ConsultantPaymentsDebitsCreditsPaginationFiltersVM>(model);

                ConsultantPaymentsDebitsCreditsPaginationFiltersVM paginationFilters = new();
                paginationFilters.Filters = new ConsultantPaymentsDebitsCreditsFiltersGetAllVM();

                int numAppliedFilters = 0;
                if (consultantPaymentsDebitsCreditsPaginationFilters.Filters != null)
                {
                    foreach (var prop in consultantPaymentsDebitsCreditsPaginationFilters.Filters.GetType().GetProperties())
                    {
                        var value = prop.GetValue(consultantPaymentsDebitsCreditsPaginationFilters.Filters, null);
                        if (value is not null and not "")
                        {
                            numAppliedFilters++;
                        }
                    }
                }
                var setPagination = new PaginationFiltersBehavior();
                paginationFilters.PaginationWithoutFilters = setPagination.SetPagination(consultantPaymentsDebitsCreditsPaginationFilters.PaginationWithoutFilters, numAppliedFilters);

                if (numAppliedFilters > 0)
                {
                    paginationFilters.Filters = consultantPaymentsDebitsCreditsPaginationFilters.Filters;
                }

                var totalResults = await _unitOfWork.ConsultantPaymentsDebitsCredits.GetAllPaymentsDebitsCreditsWithFiltersAsync(paginationFilters);
                paginationFilters.PaginationWithoutFilters.Pagination.TotalResults = totalResults.totalCount;

                var data = new { PaymentsDebitsCreditsList = totalResults.debitsCredits, PaginationFilters = paginationFilters };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"There was an error fetching the list of payments debits and credits." }, success = false, result = "errorGet", detail = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateDebitCredit([FromBody] CreateUpdateConsultantPaymentDebitCreditVM debitCreditData)
        {
            try
            {
                if (debitCreditData == null)
                {
                    return BadRequest(new { error = "The object data is null, it should be a valid object.", detail = "Object is null." });
                }
                ValidateInputs validateInputs = new();

                validateInputs.ValidateNonRequiredFieldIntType("ConsultantPaymentDebitsCreditsId", "ConsultantPaymentDebitsCreditsId", debitCreditData.ConsultantPaymentDebitsCreditsId, ModelState);
                validateInputs.ValidateRequiredFieldIntType("ConsultantId", "Consultant", debitCreditData.ConsultantId, ModelState);
                validateInputs.ValidateRequiredFieldIntType("AccountingAccountId", "Accounting Account", debitCreditData.AccountingAccountId, ModelState);
                validateInputs.ValidateRequiredFieldIntType("CostCenterId", "Cost Center", debitCreditData.CostCenterId, ModelState);
                validateInputs.ValidateRequiredFieldStringValue("Detail", "Detail", debitCreditData.Detail, ModelState);
                validateInputs.ValidateRequiredFieldNumberValue("Amount", "Unit Amount", debitCreditData.Amount, ModelState);
                validateInputs.ValidateNoNegativeNumber("Amount", "Unit Amount", debitCreditData.Amount, ModelState);
                validateInputs.ValidateNumberLessOrEqualThanZero("Amount", "Unit Amount", debitCreditData.Amount, ModelState);
                validateInputs.ValidateRequiredFieldNumberValue("Quantity", "Quantity", debitCreditData.Quantity, ModelState);
                validateInputs.ValidateNoNegativeNumber("Quantity", "Quantity", debitCreditData.Quantity, ModelState);
                validateInputs.ValidateNumberLessOrEqualThanZero("Quantity", "Quantity", debitCreditData.Quantity, ModelState);
                validateInputs.ValidateDateValidFormat("ActionDateWithinFortnight", "Action Date", debitCreditData.ActionDateWithinFortnight, ModelState);
                validateInputs.ValidateRequiredFieldAnyValue("ActionDateWithinFortnight", "Action Date", debitCreditData.ActionDateWithinFortnight, ModelState);
                validateInputs.ValidateRequiredFieldStringValue("TransactionType", "Transaction Type", debitCreditData.TransactionTypeName, ModelState);

                if (ModelState.IsValid)
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var resultMessage = "";
                    var userActionedBy = claim.Value;

                    //IF IS NOT ID THEN CREATE IT
                    if (debitCreditData.ConsultantPaymentDebitsCreditsId == null)
                    {
                        var res = await _unitOfWork.ConsultantPaymentsDebitsCredits.CreateDebitCredit(userActionedBy, debitCreditData);

                        if (res.Success)
                        {
                            resultMessage = res.Message;
                        }
                        else
                        {
                            if (res.MessageType != "Validation Error")
                            {
                                return BadRequest(new { MessageType = res.MessageType, error = res.Message, result = "ErrorSaving", detail = $"The {debitCreditData.TransactionTypeName} could be saved." });
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
                        var res = await _unitOfWork.ConsultantPaymentsDebitsCredits.UpdateDebitCredit(userActionedBy, debitCreditData);
                        if (res.Success)
                        {
                            resultMessage = res.Message;
                        }
                        else
                        {
                            if (res.MessageType != "Validation Error")
                            {
                                return BadRequest(new { error = res.Message, MessageType = res.MessageType, result = "ErrorSaving", detail = "The debit/credit could not be updated." });
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

        [HttpGet]
        public async Task<IActionResult> GetDebitCreditDataById(int consultantPaymentDebitsCreditsId)
        {
            try
            {
                var debitCreditData = await _unitOfWork.ConsultantPaymentsDebitsCredits.GetDebitCreditDataById(consultantPaymentDebitsCreditsId);
                if (debitCreditData == null)
                {
                    return BadRequest(new { error = "The transaction is not longer in the database." });
                }

                return Ok(new
                {
                    debitCreditData = debitCreditData
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectDebitCredit(int consultantPaymentDebitsCreditsId)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var res = await _unitOfWork.ConsultantPaymentsDebitsCredits.RejectDebitCredit(claim.Value, consultantPaymentDebitsCreditsId);
                if (res.Success)
                {
                    return Ok(new { success = true, message = res.Message });
                }
                else
                {
                    return BadRequest(new { error = res.Message, MessageType = res.MessageType, result = "ErrorSaving", detail = "The transaction could not be rejected." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error in the server, the transaction could not be rejected.", result = "ErrorDeleting", detail = ex.Message });
            }
        }
    }
}
