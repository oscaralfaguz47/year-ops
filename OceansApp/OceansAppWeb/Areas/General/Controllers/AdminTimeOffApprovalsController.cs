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
    [Authorize(Policy = "AccessToAdminTimeOffApprovals")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    public class AdminTimeOffApprovalsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminTimeOffApprovalsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index(int? consultantId)
        {
            ViewData["PreFilterConsultantId"] = consultantId;
            return View();
        }

        [HttpGet("GetRequests")]
        public async Task<IActionResult> GetRequests(string model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var paginationFilters = System.Text.Json.JsonSerializer
                .Deserialize<TimeOffPaginationFiltersVM>(model);

            var (requests, totalCount) = await _unitOfWork.TimeOffRequest
                .GetAdminApprovalsAsync(userId, paginationFilters);

            paginationFilters.PaginationWithoutFilters.Pagination.TotalResults = totalCount;
            return Ok(new { requestsList = requests, paginationFilters = paginationFilters });
        }

        [HttpPost("ApproveReject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveReject([FromBody] ApproveRejectTimeOffVM data)
        {
            if (data == null)
                return BadRequest(new { error = "Request data is null.", messageType = "Exception Error" });

            ValidateInputs validateInputs = new();
            validateInputs.ValidateRequiredFieldIntType("TimeOffRequestId", "Request Id",
                data.TimeOffRequestId, ModelState);
            validateInputs.ValidateRequiredFieldStringValue("TransactionStatus", "Action",
                data.TransactionStatus, ModelState);

            if (data.TransactionStatus == "Rejected")
                validateInputs.ValidateRequiredFieldStringValue("RejectionComment", "Rejection Comment",
                    data.RejectionComment, ModelState);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(new { errors = errors, messageType = "Validation Error" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string baseUrl = $"{HttpContext.Request.Scheme}://{Request.Host}";
            var response = await _unitOfWork.TimeOffRequest
                .ApproveRejectRequestAsync(userId, data, baseUrl);

            if (!response.Success)
                return BadRequest(new { error = response.Message, messageType = response.MessageType });

            return Ok(new { success = true, message = response.Message });
        }
    }
}
