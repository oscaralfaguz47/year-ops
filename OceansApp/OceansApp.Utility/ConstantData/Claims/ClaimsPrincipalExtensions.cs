
using OceansApp.Utility.ConstantData.Claims.AdminCenter;
using System.Security.Claims;

namespace OceansApp.Utility.ConstantData.Claims
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsAuthorizedForAdminCenter(this ClaimsPrincipal user)
        {
            return user.HasClaim(c => c.Type == AdminCenterClaimsCD.Actualizar_Datos_Softland_ClaimType && c.Value == AdminCenterClaimsCD.Actualizar_Datos_Softland_ClaimValue)
                   || user.HasClaim(c => c.Type == AdminCenterClaimsCD.Administracion_Usuarios_ClaimType && c.Value == AdminCenterClaimsCD.Administracion_Usuarios_ClaimValue)
                   || user.HasClaim(c => c.Type == AdminCenterClaimsCD.Roles_Permisos_Usuarios_ClaimType && c.Value == AdminCenterClaimsCD.Roles_Permisos_Usuarios_ClaimValue);
        }
    }
}
