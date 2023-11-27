
namespace OceansApp.Models.ViewModels.AdminCenter.UserRolesPermissions
{
    public class GetRolesPermissionsVM
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<GetClaimsVM>? UserClaims { get; set; }
    }
}
