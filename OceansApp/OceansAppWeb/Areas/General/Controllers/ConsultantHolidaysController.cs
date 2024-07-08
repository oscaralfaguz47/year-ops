using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Holidays;
using System.Security.Claims;

namespace OceansAppWeb.Areas.General.Controllers
{
    [ApiController]
    [Route("General/[controller]")]
    [Area("General")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    [Authorize]
    public class ConsultantHolidaysController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ConsultantHolidaysController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }
        [Authorize(Policy = "AccessToHolidaysPage")]

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "AccessToHolidaysPage")]
        [HttpGet("GetHolidaysList")]
        public async Task<IActionResult> GetHolidaysList(string model)
        {
            try
            {
                HolidaysPaginationFiltersVM holidaysPaginationFilters = System.Text.Json.JsonSerializer.Deserialize<HolidaysPaginationFiltersVM>(model);

                HolidaysPaginationFiltersVM paginationFilters = new HolidaysPaginationFiltersVM();
                paginationFilters.Filters = new HolidaysFiltersGetAllVM();

                int numAppliedFilters = 0;
                if (holidaysPaginationFilters.Filters != null)
                {
                    foreach (var prop in holidaysPaginationFilters.Filters.GetType().GetProperties())
                    {
                        var value = prop.GetValue(holidaysPaginationFilters.Filters, null);
                        if (value is not null and not "")
                        {
                            numAppliedFilters++;
                        }
                    }
                }
                var setPagination = new PaginationFiltersBehavior();
                paginationFilters.PaginationWithoutFilters = setPagination.SetPagination(holidaysPaginationFilters.PaginationWithoutFilters, numAppliedFilters);

                if (numAppliedFilters > 0)
                {
                    paginationFilters.Filters = holidaysPaginationFilters.Filters;
                }

                var totalResults = await _unitOfWork.ConsultantHoliday.GetAllHolidaysWithFiltersAsync(paginationFilters);
                paginationFilters.PaginationWithoutFilters.Pagination.TotalResults = totalResults.totalCount;

                var data = new { HolidaysList = totalResults.holidays, PaginationFilters = paginationFilters };
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"Hubo un error extrayendo la lista de Holidays." }, result = "errorGet", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToHolidaysPage")]
        [HttpGet("GetHolidayListData")]
        public async Task<IActionResult> GetHolidayListData(int holidayId)
        {
            try
            {
                var holidayData = await _unitOfWork.ConsultantHoliday.GetConsultantHolidayWithDates(holidayId);

                return Ok(new
                {
                    success = true,
                    holidayData
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", result = "error", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToHolidaysPage")]
        [HttpPost("CreateUpdateHoliday")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdateHoliday([FromBody] CreateUpdateHolidayVM holidayData)
        {
            if (holidayData == null)
            {
                return BadRequest(new { errors = new[] { "The object data is null, it should be a valid object." }, result = "ErrorObject", detail = "Object is null." });
            }
            if (string.IsNullOrEmpty(holidayData.Name) || holidayData.Name.Length > 70)
            {
                ModelState.AddModelError("Name", "The Holidays list name must be between 1 and 70 characters.");
            }
            if (holidayData.HolidayDates.Count == 0)
            {
                ModelState.AddModelError("HolidayDates", "You must enter at least one Holiday to the list.");
            }
            if (holidayData.HolidayDates.Count > 0)
            {
                for (int i = 0; i < holidayData.HolidayDates.Count; i++)
                {
                    var holidayDate = holidayData.HolidayDates[i];

                    if (string.IsNullOrEmpty(holidayDate.Name) || holidayDate.Name.Length > 70)
                    {
                        ModelState.AddModelError($"HolidayDates[{i}].Name", "The Name for Holiday #" + (i + 1) + $" must be between 1 and 70 characters.");
                    }
                    if (holidayDate.Date == null)
                    {
                        ModelState.AddModelError($"HolidayDates[{i}].Date", "The Date for Holiday #" + (i + 1) + $" is required.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var resultMessage = "";

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
                            return BadRequest(new { MessageType = res.MessageType, errors = new[] { res.Message }, result = "ErrorSaving", detail = "The Holiday list could be saved." });
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
                            return BadRequest(new { errors = new[] { res.Message }, MessageType = res.MessageType, result = "ErrorSaving", detail = "The Holiday list could be updated." });
                        }
                    }
                    return Ok(new
                    {
                        success = true,
                        message = resultMessage
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = "Error", result = "error", MessageType = "Exception Error", errors = new[] { $"There was an error creating the Holiday. More details: " + ex.Message } });
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

        [Authorize(Policy = "AccessToHolidaysPage")]
        [HttpPost("DeleteHolidaysList")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHolidaysList([FromForm] int holidaysListId)
        {
            try
            {
                var res = await _unitOfWork.ConsultantHoliday.DeleteHolidaysList(holidaysListId);
                if (!res.Success)
                {
                    return BadRequest(new { error = res.Message, messageType = res.MessageType });
                }
                return Ok(new { success = true, message = res.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error in the server, the holidays list could not be deleted.", result = "ErrorDeleting", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToListOfHolidaysForSelect")]
        [HttpGet("GetHolidaysListForSelect")]
        public async Task<IActionResult> GetHolidaysListForSelect()
        {
            try
            {
                var holidays = await _unitOfWork.ConsultantHoliday.GetAllAsync();
                var holidaysList = holidays.Select(holiday => new GetDataForSelectVM
                {
                    Value = holiday.ConsultantHolidayId,
                    Text = holiday.Name
                }).ToList();

                return Ok(new { Holidays = holidaysList });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

    }
}
