using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.AdminCenter.UserRolesPermissions;
using System.Security.Claims;

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
            var roleList = _roleManager.Roles.ToList().OrderBy(x=>x.Name);
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
        public async Task<IActionResult> GetPermissionsWhereRoleList(string roleId)
        {
            try
            {
                List<GetClaimsVM> permissionsList = new List<GetClaimsVM>();
                permissionsList = await _unitOfWork.ApplicationSystemClaim.GetClaimsListWhereRole(roleId);

                var role = _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    return BadRequest(new { message = "Error al encontrar el rol", result = "error", detail = "El rol probablemente acaba de ser eliminado de la base de datos" });
                }

                var rolesPermissionsList = new GetRolesPermissionsVM()
                {
                    RoleName = role.Result.Name,
                    RoleId = roleId,
                    UserClaims = permissionsList.ToList()
                };
                return Ok(rolesPermissionsList);
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = "Error al traer los datos", result = "error", detail = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPermissionsList()
        {
            try
            {
                var permissionsList = _unitOfWork.ApplicationSystemClaim.GetAllPermissionsCustomData();
                var rolesPermissionsList = new GetRolesPermissionsVM()
                {
                    RoleName = "",
                    RoleId = "",
                    UserClaims = (List<GetClaimsVM>)permissionsList
                };
                return Ok(rolesPermissionsList);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al traer los datos", result = "error", detail = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole([FromBody] CreateNewRoleVM roleData)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingRole = await _roleManager.FindByNameAsync(roleData.RoleName.Trim());
                    if (existingRole != null)
                    {
                        return BadRequest(new { errors = new[] { $"Ya existe un rol con el nombre '{roleData.RoleName.Trim()}'." }, result = "duplicated" });
                    }

                    var res = await _roleManager.CreateAsync(new IdentityRole(roleData.RoleName.Trim()));

                    if (res.Succeeded)
                    {
                        var createdRole = await _roleManager.FindByNameAsync(roleData.RoleName.Trim());
                        var claimsIdentity = (ClaimsIdentity)User.Identity;
                        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                        var costaRicaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central America Standard Time");

                        foreach (var permission in roleData.PermissionsList)
                        {
                            var existingClaim = await _unitOfWork.ApplicationSystemClaim.GetFirstOrDefaultAsync(x => x.ClaimId == permission.ClaimId);

                            if (existingClaim != null)
                            {
                                var roleClaim = new ApplicationRoleClaim
                                {
                                    RoleId = createdRole.Id,
                                    ClaimType = existingClaim.ClaimType,
                                    ClaimValue = existingClaim.ClaimValue,
                                    CreatedBy = claim.Value,
                                    CreationDate = costaRicaTime,
                                    UpdatedBy = claim.Value,
                                    UpdatedDate = costaRicaTime
                                };

                                var addClaimResult = await _roleManager.AddClaimAsync(createdRole, new Claim(existingClaim.ClaimType, existingClaim.ClaimValue));

                                if (!addClaimResult.Succeeded)
                                {
                                    return BadRequest(new { message = "Error al agregar el RoleClaim", result = "error", detail = addClaimResult.Errors });
                                }
                            }
                            else
                            {
                                return BadRequest(new { message = "No se pudo encontrar el claim", result = "error" });
                            }
                        }
                    }
                    else
                    {
                        return BadRequest(new { message = "Error al crear el rol", result = "error", detail = res.Errors });
                    }

                    return Ok(new { message = "El rol fue creado con éxito!", result = "success" });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = "No se pudo crear el rol", result = "error", detail = ex.Message });
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { message = "Error de validación", result = "error", errors = errors });
            }
        }

    }
}
