using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.AdminCenter.UserRolesPermissions;
using System.Security.Claims;

namespace OceansAppWeb.Areas.AdminCenter.Controllers
{
    [Area("AdminCenter")]
    [EnableCors("AllowSpecificOrigin")]
    [RequireTwoFactorEnabled]
    [Authorize(Policy = "AccessToUserRolesAndPermissions")]
    public class UserRolesPermissionsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<IdentityRole> _roleManager;
        UserManager<IdentityUser> _userManager;
        public UserRolesPermissionsController(IUnitOfWork unitOrWork, RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOrWork;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<List<GetRolesPermissionsVM>> GetRolePermissionsList()
        {
            var roleList = _roleManager.Roles.ToList().OrderBy(x => x.Name);
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
            catch (Exception ex)
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
        public async Task<IActionResult> CreateUpdateRole([FromBody] CreateNewRoleVM roleData)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var costaRicaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central America Standard Time");
                    if (roleData.RoleId == "") //IF IS NOT ROLE ID THEN CREATE THE ROLE
                    {
                        var existingRole = await _roleManager.FindByNameAsync(roleData.RoleName.Trim());
                        if (roleData.RoleId == "" && existingRole != null)
                        {
                            return BadRequest(new { errors = new[] { $"Ya existe un rol con el nombre '{roleData.RoleName.Trim()}'." }, result = "duplicated" });
                        }
                        var res = await _roleManager.CreateAsync(new IdentityRole(roleData.RoleName.Trim()));

                        if (res.Succeeded)
                        {
                            var createdRole = await _roleManager.FindByNameAsync(roleData.RoleName.Trim());
                            foreach (var permission in roleData.PermissionsList)
                            {
                                if (permission.IsAddedToTheRole)
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
                                        try
                                        {
                                            _unitOfWork.ApplicationRoleClaim.Add(roleClaim);
                                            _unitOfWork.Save();
                                        }
                                        catch (Exception ex)
                                        {
                                            return BadRequest(new { errors = new[] { $"Error al agregar el RoleClaim." }, result = "error saving", detail = ex });
                                        }
                                    }
                                    else
                                    {
                                        return BadRequest(new { errors = new[] { $"No se pudo encontrar el claim." }, result = "error saving", detail = "Claim no encontrado en la base de datos." });
                                    }
                                }
                            }
                        }
                        else
                        {
                            return BadRequest(new { errors = new[] { $"Hubo un error creando el rol." }, result = "error saving", detail = "Hubo un error mientras se guardaba el rol." });
                        }
                        return Ok(new { message = "El rol fue creado con éxito!", result = "success" });
                    }
                    else //IF ROLEID THEN EDIT THE ROLE
                    {
                        var existingRole = await _roleManager.FindByIdAsync(roleData.RoleId);
                        if (existingRole == null)
                        {
                            return BadRequest(new { errors = new[] { $"El rol no fue encontrado en la base de datos." }, result = "NotFound", detail = "El rol ya no existe en la base de datos." });
                        }

                        if (existingRole.Name != roleData.RoleName)
                        {
                            var rolWithSameName = await _roleManager.FindByNameAsync(roleData.RoleName.Trim());
                            if (rolWithSameName != null)
                            {
                                return BadRequest(new { errors = new[] { $"Ya existe un rol con el nombre '{roleData.RoleName.Trim()}'." }, result = "duplicated" });
                            }
                            existingRole.Name = roleData.RoleName;
                            var resultUpdateRoleName = await _roleManager.UpdateAsync(existingRole);
                            if (!resultUpdateRoleName.Succeeded)
                            {
                                return BadRequest(new { errors = new[] { $"Error al actualizar el nombre al rol." }, result = "error saving" });
                            }
                        }
                        var roleClaimsInRole = await _roleManager.GetClaimsAsync(existingRole);
                        foreach (var permission in roleData.PermissionsList)
                        {
                            var rolePermissionInDB = _unitOfWork.ApplicationSystemClaim.GetFirstOrDefault(x => x.ClaimId == permission.ClaimId);
                            if (rolePermissionInDB == null)
                            {
                                return BadRequest(new { errors = new[] { $"El permiso o Claim no fue encontrado en la base de datos." }, result = "NotFound", detail = "El claim no existe en la base de datos." });
                            }
                            var claimExist = roleClaimsInRole.Any(c => c.Type == rolePermissionInDB.ClaimType && c.Value == rolePermissionInDB.ClaimValue);
                            if (claimExist)
                            {
                                if (!permission.IsAddedToTheRole)
                                {
                                    var removeClaimResult = await _roleManager.RemoveClaimAsync(existingRole, new Claim(rolePermissionInDB.ClaimType, rolePermissionInDB.ClaimValue));
                                    if (!removeClaimResult.Succeeded)
                                    {
                                        return BadRequest(new { errors = new[] { $"Error al eliminar el permiso: " + rolePermissionInDB.Description + " al rol." }, result = "ErrorDeleting", detail = "No se pudo eliminar el permiso" });
                                    }
                                }
                            }
                            else
                            {
                                if (permission.IsAddedToTheRole)
                                {
                                    var roleClaimToAdd = new ApplicationRoleClaim
                                    {
                                        RoleId = existingRole.Id,
                                        ClaimType = rolePermissionInDB.ClaimType,
                                        ClaimValue = rolePermissionInDB.ClaimValue,
                                        CreatedBy = claim.Value,
                                        CreationDate = costaRicaTime,
                                        UpdatedBy = claim.Value,
                                        UpdatedDate = costaRicaTime
                                    };
                                    try
                                    {
                                        _unitOfWork.ApplicationRoleClaim.Add(roleClaimToAdd);
                                        _unitOfWork.Save();
                                    }
                                    catch (Exception ex)
                                    {
                                        return BadRequest(new { errors = new[] { $"Hubo un error agregando el permiso al rol." }, result = "ErrorSaving", detail = ex });
                                    }
                                }
                            }
                        }
                        return Ok(new { message = "El rol fue actualizado con éxito!", result = "success" });
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new { errors = new[] { $"Hubo un error al creando el rol, no se pudo guardar." }, result = "ErrorSaving", detail = ex });
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            try
            {
                var roleToDelete = await _roleManager.FindByIdAsync(roleId);
                if (roleToDelete == null)
                {
                    return BadRequest(new { errors = new[] { $"El rol no fue encontrado en la base de datos." }, result = "NotFound", detail = "El rol ya no existe en la base de datos." });
                }
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleToDelete.Name);
                if (usersInRole.Count > 0)
                {
                    return BadRequest(new { errors = new[] { $"Este rol ya está asignado a " + usersInRole.Count + " usuarios, para eliminarlo debes de remover el rol al usuario." }, result = "ErrorDelete", detail = "El rol está asignado a usuarios." });
                }
                var roleClaimsInRole = await _roleManager.GetClaimsAsync(roleToDelete);
                foreach (var claim in roleClaimsInRole)
                {
                    var removeClaimResult = await _roleManager.RemoveClaimAsync(roleToDelete, new Claim(claim.Type, claim.Value));
                    if (!removeClaimResult.Succeeded)
                    {
                        return BadRequest(new { errors = new[] { $"Error al eliminar el claim." }, result = "ErrorDeleting", detail = "No se pudo eliminar el claim o permiso." });
                    }
                }
                var resultDeleteRole = await _roleManager.DeleteAsync(roleToDelete);
                if (!resultDeleteRole.Succeeded)
                {
                    return BadRequest(new { errors = new[] { $"Hubo un error a la hora de eliminar el rol." }, result = "ErrorDeleting", detail = "El rol no pudo ser eliminado." });
                }
                return Ok(new { message = "El rol y todos sus permisos fueron eliminados con éxito!", result = "success" });
            }
            catch(Exception ex)
            {
                return BadRequest(new { errors = new[] { $"Hubo un error en la conexión con el servidor, el rol no se pudo eliminar." }, result = "ErrorDeleting", detail = ex });
            }
        }

    }
}
