using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.JournalAccountsPayable;
using OceansApp.Models.ViewModels.PaymentBookEntries;
using OceansApp.Utility.SharedMethods.InputValidations;

namespace OceansAppWeb.Areas.Finances.Controllers
{
    [ApiController]
    [Route("Finances/[controller]")]
    [Area("Finances")]
    [Authorize]
    [Authorize(Policy = "AccessToExportAccountingData")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    public class ExportAccountingDataController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ExportAccountingDataController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetAccountsPayablePartial")]
        public IActionResult GetAccountsPayablePartial()
        {
            return PartialView("_AccountsPayablePartialView");
        }
        [HttpGet("GetBookEntriesPartial")]
        public IActionResult GetBookEntriesPartial()
        {
            return PartialView("_BookEntriesPartialView");
        }

        [HttpGet("GetJournalAccountsPayableList")]
        public async Task<IActionResult> GetJournalAccountsPayableList(string model)
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
                            validateInputs.ValidateNotRequiredAndStringLength("CompanyId", "CompanyId", jsonToValidate["Filters"]["CompanyId"].ToString(), 100, ModelState);
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

                JournalAccountsPayablePaginationFiltersVM journalAccountsPayablePaginationFilters = System.Text.Json.JsonSerializer.Deserialize<JournalAccountsPayablePaginationFiltersVM>(model);

                JournalAccountsPayablePaginationFiltersVM paginationFilters = new();
                paginationFilters.Filters = new JournalAccountsPayableFiltersGetAllVM();

                int numAppliedFilters = 0;
                if (journalAccountsPayablePaginationFilters.Filters != null)
                {
                    foreach (var prop in journalAccountsPayablePaginationFilters.Filters.GetType().GetProperties())
                    {
                        var value = prop.GetValue(journalAccountsPayablePaginationFilters.Filters, null);
                        if (value is not null and not "")
                        {
                            numAppliedFilters++;
                        }
                    }
                }
                var setPagination = new PaginationFiltersBehavior();
                paginationFilters.PaginationWithoutFilters = setPagination.SetPagination(journalAccountsPayablePaginationFilters.PaginationWithoutFilters, numAppliedFilters);

                if (numAppliedFilters > 0)
                {
                    paginationFilters.Filters = journalAccountsPayablePaginationFilters.Filters;
                }

                var totalResults = await _unitOfWork.JournalAccountPayable.GetAllJournalAccountsPayableWithFiltersAsync(paginationFilters);
                paginationFilters.PaginationWithoutFilters.Pagination.TotalResults = totalResults.totalCount;

                var data = new { journalAccountsPayableList = totalResults.journalAccountsPayable, PaginationFilters = paginationFilters };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"There was an error fetching the list of journal accounts payable." }, success = false });
            }
        }

        [HttpGet("GetBookEntriesList")]
        public async Task<IActionResult> GetBookEntriesList(string model)
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
                            validateInputs.ValidateNotRequiredAndStringLength("CompanyId", "CompanyId", jsonToValidate["Filters"]["CompanyId"].ToString(), 100, ModelState);
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

                BookEntriesPaginationFiltersVM bookEntriesPaginationFilters = System.Text.Json.JsonSerializer.Deserialize<BookEntriesPaginationFiltersVM>(model);

                BookEntriesPaginationFiltersVM paginationFilters = new();
                paginationFilters.Filters = new BookEntriesFiltersGetAllVM();

                int numAppliedFilters = 0;
                if (bookEntriesPaginationFilters.Filters != null)
                {
                    foreach (var prop in bookEntriesPaginationFilters.Filters.GetType().GetProperties())
                    {
                        var value = prop.GetValue(bookEntriesPaginationFilters.Filters, null);
                        if (value is not null and not "")
                        {
                            numAppliedFilters++;
                        }
                    }
                }
                var setPagination = new PaginationFiltersBehavior();
                paginationFilters.PaginationWithoutFilters = setPagination.SetPagination(bookEntriesPaginationFilters.PaginationWithoutFilters, numAppliedFilters);

                if (numAppliedFilters > 0)
                {
                    paginationFilters.Filters = bookEntriesPaginationFilters.Filters;
                }

                var totalResults = await _unitOfWork.PaymentBookEntryParent.GetAllBookEntriesWithFiltersAsync(paginationFilters);
                paginationFilters.PaginationWithoutFilters.Pagination.TotalResults = totalResults.totalCount;

                var data = new { bookEntriesList = totalResults.bookEntries, PaginationFilters = paginationFilters };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"There was an error fetching the list of book entries." }, success = false });
            }
        }
    }
}
