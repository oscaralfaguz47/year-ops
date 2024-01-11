using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Holidays;
using System.Security.Claims;

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

        [HttpGet]
        public async Task<IActionResult> GetHolidayListData(int holidayId)
        {
            try
            {
               var permissionsList = await _unitOfWork.ConsultantHoliday.GetConsultantHolidayWithDates(holidayId);

          
                return Ok(permissionsList);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al traer los datos", result = "error", detail = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateHoliday([FromBody] CreateUpdateHolidayVM holidayData)
        {
            if (ModelState.IsValid)
            {
                if (holidayData.HolidayDates.Count == 0)
                {
                    return BadRequest(new { errors = new[] { "You must add at least one day to this Holidays list." }, result = "ErrorValidation", detail = "Holiday list date is required." });
                }
                try
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var resultMessage = ""; 

                    var costaRicaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central America Standard Time");
                    //IF IS NOT HOLIDAY ID THEN CREATE THE HOLIDAY
                    if (holidayData.ConsultantHolidayId == null) 
                    {
                        holidayData.CreatedBy = claim.Value;

                        var res = await _unitOfWork.ConsultantHoliday.CreateHolidayListWithHolidayDates(holidayData);

                        if (res.Success)
                        {
                            resultMessage = res.Message;
                        }
                        else
                        {
                            return BadRequest(new { errors = new[] { res.Message }, result = "ErrorSaving", detail = "The Holiday list could be saved." });
                        }
                    }
                    else
                    {
                        //IF IS HOLIDAY ID THEN UPDATE THE HOLIDAY
                        var res = await _unitOfWork.ConsultantHoliday.UpdateHolidayListWithHolidayDates(holidayData, claim.Value);

                        if (res.Success)
                        {
                            resultMessage = res.Message;
                        }
                        else
                        {
                            return BadRequest(new { errors = new[] { res.Message }, result = "ErrorSaving", detail = "The Holiday list could be updated." });
                        }
                    }
                    return Json(new
                    {
                        success = true,
                        message = resultMessage
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { errors = new[] { $"There was an error creating the Holiday." }, result = "ErrorSaving", detail = ex });
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { message = "Validation Error", result = "error", errors = errors });
            }
        }
    }
}
