using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Utility.SharedMethods.InputValidations;

namespace OceansAppWeb.Areas.General.Controllers
{
    [ApiController]
    [Route("General/[controller]")]
    [Area("General")]
    [Authorize]
    [RequireTwoFactorEnabled]
    public class ConsultantDetailsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthorizationService _authorizationService;
        public ConsultantDetailsController(IUnitOfWork unitOrWork, IAuthorizationService authorizationService)
        {
            _unitOfWork = unitOrWork;
            _authorizationService = authorizationService;
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "AccessToSuccessManagersListForSelect")]
        [HttpGet("GetSuccessManagers")]
        public async Task<IActionResult> GetSuccessManagers()
        {
            try
            {
                var users = await _unitOfWork.ConsultantDetail.GetUsersByCategoryAndPositionForSelect("Administrative", "Success Manager");
                return Ok(new
                {
                    SuccessManagers = users
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
        
        [Authorize(Policy = "AccessToSearchConsultantsBySearchText")]
        [HttpGet("GetConsultantsBySearchText")]
        public async Task<IActionResult> GetConsultantsBySearchText(string? searchText)
        {
            try
            {
                ValidateInputs validateInputs = new();
                validateInputs.ValidateNotRequiredAndStringLength("SearchConsultant", "Search Consultant", searchText != null ? searchText.Trim() : searchText, 100, ModelState);
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();
                    return BadRequest(new { MessageType = "Validation Error", message = "Validation Error", result = "error", errors = errors, detail = "Parameters for filters are not correct." });
                }
                var authToManageAdminitrativeConsultants = await _authorizationService.AuthorizeAsync(User, "AccessToManageAdministrativeConsultants");
                string? userCategoryName = "Consultant";
                if (authToManageAdminitrativeConsultants.Succeeded)
                {
                    userCategoryName = null;
                }
                var consultants = await _unitOfWork.ConsultantDetail.GetConsultantsBySearchText(searchText != null ? searchText.Trim() : searchText, userCategoryName);
                return Ok(new
                {
                    Consultants = consultants
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToSearchAllActiveConsultantsBySearchText")]
        [HttpGet("GetAllActiveConsultantsBySearchText")]
        public async Task<IActionResult> GetAllActiveConsultantsBySearchText(string? searchText, string? userCategoryName)
        {
            try
            {
                ValidateInputs validateInputs = new();
                validateInputs.ValidateNotRequiredAndStringLength("SearchConsultant", "Search Consultant", searchText != null ? searchText.Trim() : searchText, 100, ModelState);
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();
                    return BadRequest(new { MessageType = "Validation Error", message = "Validation Error", result = "error", errors = errors, detail = "Parameters for filters are not correct." });
                }
                var consultants = await _unitOfWork.ConsultantDetail.GetConsultantsBySearchText(searchText != null ? searchText.Trim() : searchText, userCategoryName);
                return Ok(new
                {
                    Consultants = consultants
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

    }
}
