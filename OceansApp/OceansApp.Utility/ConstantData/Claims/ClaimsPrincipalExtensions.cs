
using OceansApp.Utility.ConstantData.Claims.AdminCenter;
using OceansApp.Utility.ConstantData.Claims.Finances;
using OceansApp.Utility.ConstantData.Claims.General;
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

        public static bool IsAuthorizedForGeneral(this ClaimsPrincipal user)
        {
            return false;
           // return user.HasClaim(c => c.Type == ConsultantsClaimsCD.Consultants_Page_ClaimType && c.Value == ConsultantsClaimsCD.Consultants_Page_ClaimValue);
        }
        public static bool IsAuthorizedForFinances(this ClaimsPrincipal user)
        {
            return user.HasClaim(c => c.Type == FinancesClaimsCD.Accounts_Receivable_ClaimType && c.Value == FinancesClaimsCD.Accounts_Receivable_ClaimValue);
        }
    }
}
