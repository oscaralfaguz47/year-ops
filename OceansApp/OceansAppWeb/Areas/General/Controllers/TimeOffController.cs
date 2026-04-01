using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.TimeOff;
using OceansApp.Utility.SharedMethods.InputValidations;
using OceansAppWeb;
using System.Security.Claims;

namespace OceansAppWeb.Areas.General.Controllers
{
    [ApiController]
    [Route("General/[controller]")]
    [Area("General")]
    [Authorize]
    [Authorize(Policy = "AccessToTimeOffRequest")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    public class TimeOffController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public TimeOffController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetCalendarEntries")]
        public async Task<IActionResult> GetCalendarEntries(DateTime monthStart, DateTime monthEnd)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var consultant = await _unitOfWork.ConsultantDetail
                .GetFirstOrDefaultAsync(c => c.UserId == userId);
            if (consultant == null)
                return BadRequest(new { error = "Consultant not found.", messageType = "Exception Error" });

            var entries = await _unitOfWork.TimeOffRequest
                .GetCalendarEntriesAsync(consultant.ConsultantId, monthStart, monthEnd);
            return Ok(new { calendarEntries = entries });
        }

        [HttpGet("GetBalances")]
        public async Task<IActionResult> GetBalances()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var consultant = await _unitOfWork.ConsultantDetail
                .GetFirstOrDefaultAsync(c => c.UserId == userId);
            if (consultant == null)
                return BadRequest(new { error = "Consultant not found.", messageType = "Exception Error" });

            var balances = await _unitOfWork.TimeOffRequest
                .GetBalancesAsync(consultant.ConsultantId);
            return Ok(new { balances = balances });
        }

        [HttpGet("GetHolidayDates")]
        public async Task<IActionResult> GetHolidayDates()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var consultant = await _unitOfWork.ConsultantDetail
                .GetFirstOrDefaultAsync(c => c.UserId == userId);
            if (consultant == null)
                return BadRequest(new { error = "Consultant not found.", messageType = "Exception Error" });

            var holidays = await _unitOfWork.TimeOffRequest
                .GetConsultantHolidayDatesAsync(consultant.ConsultantId);
            return Ok(new { holidayDates = holidays });
        }

        [HttpGet("GetMyRequests")]
        public async Task<IActionResult> GetMyRequests(string model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var consultant = await _unitOfWork.ConsultantDetail
                .GetFirstOrDefaultAsync(c => c.UserId == userId);
            if (consultant == null)
                return BadRequest(new { error = "Consultant not found.", messageType = "Exception Error" });

            var paginationFilters = System.Text.Json.JsonSerializer
                .Deserialize<TimeOffPaginationFiltersVM>(model);

            var (requests, totalCount) = await _unitOfWork.TimeOffRequest
                .GetConsultantRequestsAsync(consultant.ConsultantId, paginationFilters);

            paginationFilters.PaginationWithoutFilters.Pagination.TotalResults = totalCount;
            return Ok(new { requestsList = requests, paginationFilters = paginationFilters });
        }

        [HttpPost("SubmitRequest")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRequest([FromBody] SubmitTimeOffRequestVM data)
        {
            if (data == null)
                return BadRequest(new { error = "Request data is null.", messageType = "Exception Error" });

            ValidateInputs validateInputs = new();
            validateInputs.ValidateRequiredFieldStringValue("TimeOffType", "Time Off Type", data.TimeOffType, ModelState);
            validateInputs.ValidateDateValidFormat("StartDate", "Start Date", data.StartDate?.ToString(), ModelState);
            validateInputs.ValidateDateValidFormat("EndDate", "End Date", data.EndDate?.ToString(), ModelState);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(new { errors = errors, messageType = "Validation Error" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var consultant = await _unitOfWork.ConsultantDetail
                .GetFirstOrDefaultAsync(c => c.UserId == userId);
            if (consultant == null)
                return BadRequest(new { error = "Consultant not found.", messageType = "Exception Error" });

            string baseUrl = $"{HttpContext.Request.Scheme}://{Request.Host}";
            var response = await _unitOfWork.TimeOffRequest
                .SubmitRequestAsync(userId, consultant.ConsultantId, data, baseUrl);

            if (!response.Success)
                return BadRequest(new { error = response.Message, messageType = response.MessageType });

            return Ok(new { success = true, message = response.Message });
        }
    }
}
