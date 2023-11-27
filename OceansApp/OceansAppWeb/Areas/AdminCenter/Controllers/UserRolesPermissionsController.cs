using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.AdminCenter.UserRolesPermissions;

namespace OceansAppWeb.Areas.AdminCenter.Controllers
{
    [Area("AdminCenter")]
    [Authorize(Policy = "AccessToUserRolesAndPermissions")]
    [RequireTwoFactorEnabled]
    public class UserRolesPermissionsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserRolesPermissionsController(IUnitOfWork unitOrWork, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOrWork;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<List<GetRolesPermissionsVM>> ObtenerListaJson()
        {
            var roleList = _roleManager.Roles.ToList();
            List<GetRolesPermissionsVM> rolesPermissionsList = new List<GetRolesPermissionsVM>();
            foreach (var role in roleList)
            {
                var userClaimList = await _unitOfWork.ApplicationSystemClaim.GetClaimsListWhereRole(role.Id);
                rolesPermissionsList.Add(new GetRolesPermissionsVM() { 
                    RoleId = role.Id,
                    RoleName = role.Name,
                    UserClaims = userClaimList.ToList()
                });
            }

            // Devolver la lista como JSON
            return rolesPermissionsList;
        }
    }
}
