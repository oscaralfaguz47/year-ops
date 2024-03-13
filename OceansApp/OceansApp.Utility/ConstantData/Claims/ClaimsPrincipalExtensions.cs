
using OceansApp.Utility.ConstantData.Claims.AdminCenter;
using OceansApp.Utility.ConstantData.Claims.Finances;
using OceansApp.Utility.ConstantData.Claims.General;
using OceansApp.Utility.ConstantData.Claims.AccountManagement;
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
            return user.HasClaim(c => c.Type == ConsultantsClaimsCD.Consultants_Page_ClaimType && c.Value == ConsultantsClaimsCD.Consultants_Page_ClaimValue) ||
                user.HasClaim(c => c.Type == ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimType && c.Value == ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimValue) ||
                 user.HasClaim(c => c.Type == HolidaysClaimsCD.Holidays_Page_ClaimType && c.Value == HolidaysClaimsCD.Holidays_Page_ClaimValue) ||
                 user.HasClaim(c => c.Type == ConsultantsBenefitsClaimsCD.Consultants_Benefits_Page_ClaimType && c.Value == ConsultantsBenefitsClaimsCD.Consultants_Benefits_Page_ClaimValue);
        }
        public static bool IsAuthorizedForConsultantsPage(this ClaimsPrincipal user)
        {
            return user.HasClaim(c => c.Type == ConsultantsClaimsCD.Consultants_Page_ClaimType && c.Value == ConsultantsClaimsCD.Consultants_Page_ClaimValue) ||
                user.HasClaim(c => c.Type == ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimType && c.Value == ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimValue);
        }
        public static bool IsAuthorizedForFinances(this ClaimsPrincipal user)
        {
            return user.HasClaim(c => c.Type == FinancesClaimsCD.Accounts_Receivable_ClaimType && c.Value == FinancesClaimsCD.Accounts_Receivable_ClaimValue);
        }

        public static bool IsAuthorizedForAccountManagement(this ClaimsPrincipal user)
        {
            return user.HasClaim(c => c.Type == ClientsClaimsCD.Clients_Page_ClaimType && c.Value == ClientsClaimsCD.Clients_Page_ClaimValue)
                || user.HasClaim(c => c.Type == ProjectsClaimsCD.Projects_Page_ClaimType && c.Value == ProjectsClaimsCD.Projects_Page_ClaimValue);
        }
    }
}
