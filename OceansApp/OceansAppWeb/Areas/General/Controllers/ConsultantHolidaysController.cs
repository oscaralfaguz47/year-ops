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

        [HttpPost]
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
            if (holidayData.Year == null)
            {
                ModelState.AddModelError("Year", "The Year of the Holidays list is required.");
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHolidaysList(int holidayListId)
        {
            try
            {
                var holdayListToDelete = _unitOfWork.ConsultantHoliday.GetFirstOrDefault(x=>x.ConsultantHolidayId == holidayListId);
                if (holdayListToDelete == null)
                {
                    return BadRequest(new { error = "The holiday list does not exist in the database.", result = "NotFound", detail = "The holiday list was already deleted before your request." });
                }
                var datesInHolidayList = await _userManager.GetUsersInRoleAsync(holdayListToDelete.Name);
                if (datesInHolidayList.Count > 0)
                {
                    return BadRequest(new { errors = new[] { $"Este rol ya está asignado a " + datesInHolidayList.Count + " usuarios, para eliminarlo debes de remover el rol al usuario." }, result = "ErrorDelete", detail = "El rol está asignado a usuarios." });
                }
                var roleClaimsInRole = await _roleManager.GetClaimsAsync(holdayListToDelete);
                foreach (var claim in roleClaimsInRole)
                {
                    var removeClaimResult = await _roleManager.RemoveClaimAsync(holdayListToDelete, new Claim(claim.Type, claim.Value));
                    if (!removeClaimResult.Succeeded)
                    {
                        return BadRequest(new { errors = new[] { $"Error al eliminar el claim." }, result = "ErrorDeleting", detail = "No se pudo eliminar el claim o permiso." });
                    }
                }
                var resultDeleteRole = await _roleManager.DeleteAsync(holdayListToDelete);
                if (!resultDeleteRole.Succeeded)
                {
                    return BadRequest(new { errors = new[] { $"Hubo un error a la hora de eliminar el rol." }, result = "ErrorDeleting", detail = "El rol no pudo ser eliminado." });
                }
                return Ok(new { message = "El rol y todos sus permisos fueron eliminados con éxito!", result = "success" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { errors = new[] { $"Hubo un error en la conexión con el servidor, el rol no se pudo eliminar." }, result = "ErrorDeleting", detail = ex });
            }
        }
    }
}
