using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.PaymentSheets;
using OceansApp.Utility.SharedMethods.InputValidations;

namespace OceansAppWeb.Areas.Finances.Controllers
{
    [Area("Finances")]
    [Authorize]
    [Authorize(Policy = "AccessToManageTheBasicsOfPaymentSheets")]
    [RequireTwoFactorEnabled]
    public class PaymentSheetsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public PaymentSheetsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
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
                            validateInputs.ValidateNonRequiredFieldIntType("TransactionStatusId", "Transaction Status", jsonToValidate["Filters"]["TransactionStatusId"], ModelState);
                            validateInputs.ValidateNonRequiredFieldIntType("ProjectId", "Project", jsonToValidate["Filters"]["ProjectId"], ModelState);
                            validateInputs.ValidateNonRequiredFieldIntType("PaymentPeriod", "Payment Period", jsonToValidate["Filters"]["PaymentPeriod"], ModelState);

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
    }
}
