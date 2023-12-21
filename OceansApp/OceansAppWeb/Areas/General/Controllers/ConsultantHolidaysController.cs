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
        public ActionResult ActualizarPagination(Pagination modelo)
        {
            // Procesar el modelo o preparar los datos necesarios para el Partial View
            return PartialView("_Pagination", modelo);
        }

        [HttpGet]
        public async Task<IActionResult> GetHolidaysList(string model)
        {
            try
            {
                HolidaysPaginationFiltersVM holidaysPaginationFilters = System.Text.Json.JsonSerializer.Deserialize<HolidaysPaginationFiltersVM>(model);

                var paginationFilters = new HolidaysPaginationFiltersVM
                {
                    Pagination = holidaysPaginationFilters.Pagination ?? new Pagination(),
                    Filters = holidaysPaginationFilters.Filters ?? new HolidaysFiltersGetAllVM()
                };

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
