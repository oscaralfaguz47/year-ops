using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public async Task<List<GetRolesPermissionsVM>> GetRolePermissionsList()
        {
            var roleList = _roleManager.Roles.ToList();
            List<GetRolesPermissionsVM> rolesPermissionsList = new List<GetRolesPermissionsVM>();
            foreach (var role in roleList)
            {
                var userClaimList = await _unitOfWork.ApplicationSystemClaim.GetClaimsListWhereRole(role.Id);
                rolesPermissionsList.Add(new GetRolesPermissionsVM()
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    UserClaims = userClaimList.ToList()
                });
            }
            return rolesPermissionsList;
        }

        [HttpGet]
        public ActionResult<IEnumerable<GetPermissionsListVM>> GetPermissionsList()
        {
            var permissionsList = _unitOfWork.ApplicationSystemClaim.GetAllPermissionsCustomData();

            return Ok(permissionsList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateRole(string roleName)
        {
            try
            {
                var existingRole = _roleManager.FindByNameAsync(roleName.Trim());
                if (existingRole != null)
                {
                    return BadRequest(new { message = "Ya existe un rol con el nombre '" + roleName.Trim() + "'.", result = "duplicated" });
                }
                _roleManager.CreateAsync(new IdentityRole(roleName.Trim())).GetAwaiter().GetResult();
                return Ok(new { message = "El rol fue creado con éxito!", result = "success" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "No se pudo crear el rol", result = "error", detail = ex.Message });
            }
        }
    }
}
