
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels.AdminCenter.UserRolesPermissions
{
    public class CreateNewRoleVM
    {
        public List<CreateRolePermissionsVM> PermissionsList { get; set; }
        [Required(ErrorMessage = "Role name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "The role name must be between 1 and 100 characters.")]
        public string RoleName { get; set; }
        public string RoleId { get; set; }
    }
}
