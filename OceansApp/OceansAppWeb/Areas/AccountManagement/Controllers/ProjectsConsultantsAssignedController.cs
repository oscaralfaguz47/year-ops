using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using System.Security.Claims;

namespace OceansAppWeb.Areas.AccountManagement.Controllers
{
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
        [HttpGet]
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
    }
}
