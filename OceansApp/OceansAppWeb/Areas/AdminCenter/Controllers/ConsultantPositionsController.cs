using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.PaymentSheets;
using OceansApp.Utility.SharedMethods.InputValidations;

namespace OceansAppWeb.Areas.AdminCenter.Controllers
{
    [ApiController]
    [Route("AdminCenter/[controller]")]
    [Area("AdminCenter")]
    [EnableCors("AllowSpecificOrigin")]
    [RequireTwoFactorEnabled]
    [Authorize]
    public class ConsultantPositionsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ConsultantPositionsController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        [Authorize(Policy = "AccessToConsultantPositions")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetConsultantPositionsList")]
        public async Task<IActionResult> GetConsultantPositionsList(string model)
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

        [Authorize(Policy = "AccessToAllConsultantPositionsList")]
        [HttpGet("GetAllConsultantPositionsListForSelect")]
        public async Task<IActionResult> GetAllConsultantPositionsListForSelect(bool isAdministrative)
        {
            try
            {
                List<GetDataForSelectVM> positionsList = await _unitOfWork.ConsultantPosition.GetPositionsByIsAdministrative(isAdministrative);

                return Ok(new
                {
                    Positions = positionsList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
    }
}
