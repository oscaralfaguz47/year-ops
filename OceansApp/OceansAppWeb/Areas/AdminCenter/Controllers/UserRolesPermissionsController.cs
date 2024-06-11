using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.AdminCenter.UserRolesPermissions;
using OceansApp.Models.ViewModels.Components;
using System.Security.Claims;

namespace OceansAppWeb.Areas.AdminCenter.Controllers
{
    [ApiController]
    [Route("AdminCenter/[controller]")]
    [Area("AdminCenter")]
    [EnableCors("AllowSpecificOrigin")]
    [RequireTwoFactorEnabled]
    [Authorize]
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

        [Authorize(Policy = "AccessToUserRolesAndPermissions")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "AccessToUserRolesAndPermissions")]
        [HttpGet("GetRolePermissionsList")]
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

        [Authorize(Policy = "AccessToUserRolesAndPermissions")]
        [HttpGet("GetPermissionsWhereRoleList")]
        public async Task<IActionResult> GetPermissionsWhereRoleList(string roleId)
        {
            try
            {
                List<GetClaimsVM> permissionsList = new List<GetClaimsVM>();
                permissionsList = await _unitOfWork.ApplicationSystemClaim.GetClaimsListWhereRole(roleId);

                var role = _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    return BadRequest(new { message = "The role was not found", result = "error", detail = "The role was probably just deleted from the database" });
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
                return BadRequest(new { message = "Error fetching data", result = "error", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToUserRolesAndPermissions")]
        [HttpGet("GetPermissionsList")]
        public async Task<IActionResult> GetPermissionsList()
        {
            try
            {
                var permissionsList = await _unitOfWork.ApplicationSystemClaim.GetAllPermissionsCustomData();
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
                return BadRequest(new { message = "Error fetching data", result = "error", detail = ex.Message });
            }
        }
        
        [Authorize(Policy = "AccessToAllRolesList")]
        [HttpGet("GetAllRolesListForSelect")]
        public async Task<IActionResult> GetAllRolesListForSelect()
        {
            try
            {
                List<SelectVM> rolesList = new();
                var missingRole = "";
                if (!User.IsInRole("Master"))
                {
                    missingRole = "Master";
                }
                var roles = _roleManager.Roles.ToList().Where(x => x.Name != missingRole).OrderBy(x => x.Name);
                foreach (var role in roles)
                {
                    rolesList.Add(new SelectVM { Value = role.Name, Name = role.Name });
                }
                return Ok(new
                {
                    Roles = rolesList
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving data. Please report this issue.", detail = ex.Message });
            }
        }

        [Authorize(Policy = "AccessToUserRolesAndPermissions")]
        [HttpPost("CreateUpdateRole")]
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
                            return BadRequest(new { errors = new[] { $"There is already a role with the name '{roleData.RoleName.Trim()}'." }, result = "duplicated" });
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
                                            await _unitOfWork.ApplicationRoleClaim.AddAsync(roleClaim);
                                            await _unitOfWork.SaveAsync();
                                        }
                                        catch (Exception ex)
                                        {
                                            return BadRequest(new { errors = new[] { $"Error adding RoleClaim." }, result = "error saving", detail = ex });
                                        }
                                    }
                                    else
                                    {
                                        return BadRequest(new { errors = new[] { $"The claim could not be found." }, result = "error saving", detail = "Claim not found in the database." });
                                    }
                                }
                            }
                        }
                        else
                        {
                            return BadRequest(new { errors = new[] { $"There was an error creating the role." }, result = "error saving", detail = "There was an error while saving the role." });
                        }
                        return Ok(new { message = "The role was created successfully!", result = "success" });
                    }
                    else //IF ROLEID THEN EDIT THE ROLE
                    {
                        var existingRole = await _roleManager.FindByIdAsync(roleData.RoleId);
                        if (existingRole == null)
                        {
                            return BadRequest(new { errors = new[] { $"The role was not found in the database." }, result = "NotFound", detail = "The role no longer exists in the database." });
                        }

                        if (existingRole.Name != roleData.RoleName)
                        {
                            var rolWithSameName = await _roleManager.FindByNameAsync(roleData.RoleName.Trim());
                            if (rolWithSameName != null)
                            {
                                return BadRequest(new { errors = new[] { $"There is already a role with the name '{roleData.RoleName.Trim()}'." }, result = "duplicated" });
                            }
                            existingRole.Name = roleData.RoleName;
                            var resultUpdateRoleName = await _roleManager.UpdateAsync(existingRole);
                            if (!resultUpdateRoleName.Succeeded)
                            {
                                return BadRequest(new { errors = new[] { $"Error updating name to role." }, result = "error saving" });
                            }
                        }
                        var roleClaimsInRole = await _roleManager.GetClaimsAsync(existingRole);
                        foreach (var permission in roleData.PermissionsList)
                        {
                            var rolePermissionInDB = await _unitOfWork.ApplicationSystemClaim.GetFirstOrDefaultAsync(x => x.ClaimId == permission.ClaimId);
                            if (rolePermissionInDB == null)
                            {
                                return BadRequest(new { errors = new[] { $"The permit or Claim was not found in the database." }, result = "NotFound", detail = "The claim does not exist in the database." });
                            }
                            var claimExist = roleClaimsInRole.Any(c => c.Type == rolePermissionInDB.ClaimType && c.Value == rolePermissionInDB.ClaimValue);
                            if (claimExist)
                            {
                                if (!permission.IsAddedToTheRole)
                                {
                                    var removeClaimResult = await _roleManager.RemoveClaimAsync(existingRole, new Claim(rolePermissionInDB.ClaimType, rolePermissionInDB.ClaimValue));
                                    if (!removeClaimResult.Succeeded)
                                    {
                                        return BadRequest(new { errors = new[] { $"Error removing permission: " + rolePermissionInDB.Description + " al rol." }, result = "ErrorDeleting", detail = "Could not remove permission" });
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
                                        await _unitOfWork.ApplicationRoleClaim.AddAsync(roleClaimToAdd);
                                        await _unitOfWork.SaveAsync();
                                    }
                                    catch (Exception ex)
                                    {
                                        return BadRequest(new { errors = new[] { $"There was an error adding the permission to the role." }, result = "ErrorSaving", detail = ex });
                                    }
                                }
                            }
                        }
                        return Ok(new { message = "The role was successfully updated!", result = "success" });
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new { errors = new[] { $"There was an error creating the role, it could not be saved." }, result = "ErrorSaving", detail = ex });
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { message = "Validation error", result = "error", errors = errors });
            }
        }
        
        [Authorize(Policy = "AccessToUserRolesAndPermissions")]
        [HttpPost("DeleteRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            try
            {
                var roleToDelete = await _roleManager.FindByIdAsync(roleId);
                if (roleToDelete == null)
                {
                    return BadRequest(new { errors = new[] { $"The role was not found in the database." }, result = "NotFound", detail = "The role no longer exists in the database." });
                }
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleToDelete.Name);
                if (usersInRole.Count > 0)
                {
                    return BadRequest(new { errors = new[] { $"This role is already assigned to " + usersInRole.Count + " users, to delete it you must remove the user's role." }, result = "ErrorDelete", detail = "The role is assigned to users." });
                }
                var roleClaimsInRole = await _roleManager.GetClaimsAsync(roleToDelete);
                foreach (var claim in roleClaimsInRole)
                {
                    var removeClaimResult = await _roleManager.RemoveClaimAsync(roleToDelete, new Claim(claim.Type, claim.Value));
                    if (!removeClaimResult.Succeeded)
                    {
                        return BadRequest(new { errors = new[] { $"Error when deleting the claim." }, result = "ErrorDeleting", detail = "Could not delete claim or permission." });
                    }
                }
                var resultDeleteRole = await _roleManager.DeleteAsync(roleToDelete);
                if (!resultDeleteRole.Succeeded)
                {
                    return BadRequest(new { errors = new[] { $"There was an error while deleting the role." }, result = "ErrorDeleting", detail = "The role could not be deleted." });
                }
                return Ok(new { message = "The role and all its permissions were successfully deleted!", result = "success" });
            }
            catch(Exception ex)
            {
                return BadRequest(new { errors = new[] { $"There was an error connecting to the server, the role could not be deleted." }, result = "ErrorDeleting", detail = ex });
            }
        }

    }
}
