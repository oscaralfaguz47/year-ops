using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Holidays;

namespace OceansAppWeb.Areas.General.Controllers
{
    [Area("General")]
    [RequireTwoFactorEnabled]
    [Authorize(Policy = "AccessToHolidaysPage")]
    public class ConsultantHolidaysController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ConsultantHolidaysController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUniqueYears()
        {
            try
            {
                var uniqueYears = await _unitOfWork.ConsultantHoliday.GetHolidaysYears();
                return Json(uniqueYears);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new[] { $"Hubo un error extrayendo la lista de años." },
                    result = "errorGet",
                    detail = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetHolidaysList(string model)
        {
            try
            {
                HolidaysPaginationFiltersVM holidaysPaginationFilters = System.Text.Json.JsonSerializer.Deserialize<HolidaysPaginationFiltersVM>(model);

                HolidaysPaginationFiltersVM paginationFilters = new HolidaysPaginationFiltersVM();
                paginationFilters.Pagination = new Pagination();
                paginationFilters.Filters = new HolidaysFiltersGetAllVM();
                paginationFilters.OrderBy = new OrderByVM();

                if (holidaysPaginationFilters.Pagination != null && holidaysPaginationFilters.Filters != null)
                {
                    int numAppliedFilters = 0;
                    foreach (var prop in holidaysPaginationFilters.Filters.GetType().GetProperties())
                    {
                        string name = prop.Name;
                        var value = prop.GetValue(holidaysPaginationFilters.Filters, null);
                        if (value is not null and not "")
                        {
                            numAppliedFilters++;
                        }
                    }
                    if (holidaysPaginationFilters.Pagination.PageSize != 0)
                    {
                        paginationFilters.Pagination.PageSize = holidaysPaginationFilters.Pagination.PageSize;
                    }
                    if (numAppliedFilters > 0)
                    {
                        paginationFilters.Filters = holidaysPaginationFilters.Filters;
                        if (holidaysPaginationFilters.RequestFromFilters == false)
                        {
                            paginationFilters.Pagination.PageIndex = holidaysPaginationFilters.Pagination.PageIndex;
                        }
                    }
                    else
                    {
                        paginationFilters.Pagination.PageIndex = holidaysPaginationFilters.Pagination.PageIndex;
                    }
                }
                if (holidaysPaginationFilters.OrderBy != null)
                {
                    paginationFilters.OrderBy = holidaysPaginationFilters.OrderBy;
                }
                var totalResults = await _unitOfWork.ConsultantHoliday.GetAllHolidaysWithFiltersAsync(paginationFilters);
                paginationFilters.Pagination.TotalResults = totalResults.totalCount;
                HolidaysGetAllForListVM viewModel = new HolidaysGetAllForListVM
                {
                    HolidaysList = totalResults.holidays,
                    PaginationFilters = paginationFilters
                };
                string jsonResult = System.Text.Json.JsonSerializer.Serialize(viewModel);
                return Content(jsonResult, "application/json");
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"Hubo un error extrayendo la lista de Holidays." }, result = "errorGet", detail = ex.Message });
            }
        }
    }
}
