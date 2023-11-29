
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels.AdminCenter.UserRolesPermissions
{
    public class CreateNewRoleVM
    {
        public List<CreateRolePermissionsVM> PermissionsList { get; set; }
        [Required(ErrorMessage = "El nombre del rol es requerido")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 100 caracteres.")]
        public string RoleName { get; set; }
    }
}
