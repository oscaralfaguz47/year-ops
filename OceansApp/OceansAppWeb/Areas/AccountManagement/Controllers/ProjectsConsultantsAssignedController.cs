using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using System.Security.Claims;

namespace OceansAppWeb.Areas.AccountManagement.Controllers
{
    [ApiController]
    [Route("AccountManagement/[controller]")]
    [Area("AccountManagement")]
    [RequireTwoFactorEnabled]
    [Authorize]
    public class ProjectsConsultantsAssignedController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthorizationService _authorizationService;
        public ProjectsConsultantsAssignedController(IUnitOfWork unitOrWork, IAuthorizationService authorizationService)
        {
            _unitOfWork = unitOrWork;
            _authorizationService = authorizationService;
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
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null)
                {
                    return BadRequest(new { error = "User not valid." });
                }
                var projectInfoData = await _unitOfWork.ProjectConsultantAssigned.GetConsultantSelectedProjectInfo(claim.Value);

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
