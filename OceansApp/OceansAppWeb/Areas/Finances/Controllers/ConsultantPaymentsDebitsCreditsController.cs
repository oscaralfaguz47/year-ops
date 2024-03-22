using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantPaymentsDebitsCredits;
using OceansApp.Utility.SharedMethods.InputValidations;

namespace OceansAppWeb.Areas.Finances.Controllers
{
    [Area("Finances")]
    [Authorize]
    [Authorize(Policy = "AccessToManageConsultantPaymentsDebitsAndCredits")]
    [RequireTwoFactorEnabled]
    public class ConsultantPaymentsDebitsCreditsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ConsultantPaymentsDebitsCreditsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
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
    }
}
