using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Utility.SharedMethods.InputValidations;
using System.Security.Claims;

namespace OceansAppWeb.Areas.AccountManagement.Controllers
{
    [ApiController]
    [Route("AccountManagement/[controller]")]
    [Area("AccountManagement")]
    [ServiceFilter(typeof(RequireTwoFactorEnabledAttribute))]
    [Authorize]
    public class ProjectsConsultantsAssignedController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProjectsConsultantsAssignedController(IUnitOfWork unitOrWork)
        {
            _unitOfWork = unitOrWork;
        }

        [Authorize(Policy = "BasicAccessToReportingMyTime")]
        [HttpGet("GetProjectsWhereConsultantAssigned")]
        public async Task<IActionResult> GetProjectsWhereConsultantAssigned()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null)
                {
                    return BadRequest(new { error = "User not valid." });
                }

                var projects = await _unitOfWork.ProjectConsultantAssigned.GetProjectsWhereConsultantAssigned(claim.Value);

                return Ok(new
                {
                    projects = projects
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [Authorize(Policy = "BasicAccessToReportingMyTime")]
        [HttpGet("GetConsultantSelectedProjectInfo")]
        public async Task<IActionResult> GetConsultantSelectedProjectInfo()
        {
            try
            {
                string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userActionedBy == null)
                {
                    return BadRequest(new { error = "User not valid." });
                }
                var projectInfoData = await _unitOfWork.ProjectConsultantAssigned.GetConsultantSelectedProjectInfo(userActionedBy);

                return Ok(new
                {
                    projectInfoData = projectInfoData
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }
        [Authorize(Policy = "BasicAccessToReportingMyTime")]
        [HttpGet("GetConsultantStatusInTheProject")]
        public async Task<IActionResult> GetConsultantStatusInTheProject(DateTime startDate, DateTime endDate)
        {
            try
            {
                ValidateInputs validateInputs = new();
                validateInputs.ValidateDateValidFormat("StartDate", "Start Date", startDate, ModelState);
                validateInputs.ValidateDateValidFormat("EndDate", "End Date", endDate, ModelState);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();
                    return BadRequest(new { MessageType = "Validation Error", message = "Validation Error", result = "error", errors = errors });
                }

                string userActionedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userActionedBy == null)
                {
                    return BadRequest(new { error = "User not valid." });
                }
                var consultantStatus = await _unitOfWork.ProjectConsultantAssigned.GetConsultantStatusInTheProject(userActionedBy, startDate, endDate);

                return Ok(new
                {
                    consultantStatusInTheProject = consultantStatus
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [Authorize(Policy = "BasicAccessToReportingMyTime")]
        [HttpPost("SelectConsultantProject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectConsultantProject([FromForm] int projectId)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                var projectUserSelectedToDelete = await _unitOfWork.ProjectUserSelected.GetFirstOrDefaultAsync(x => x.UserId == claim.Value);
                if (projectUserSelectedToDelete == null)
                {
                    return BadRequest(new { error = "The user has not a project selected." });
                }
                var transact = await _unitOfWork.BeginTranAsync();
                _unitOfWork.ProjectUserSelected.Remove(projectUserSelectedToDelete);
                ProjectUserSelected projectUserSelectedToCreate = new()
                {
                    ProjectId = projectId,
                    UserId = claim.Value
                };
                await _unitOfWork.ProjectUserSelected.AddAsync(projectUserSelectedToCreate);
                await _unitOfWork.SaveAsync();
                transact.Commit();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"There was an error in the server, the project could not be selected.", detail = ex.Message });
            }
        }
    }
}
