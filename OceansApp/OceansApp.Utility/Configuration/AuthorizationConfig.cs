using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OceansApp.Utility.Configuration.AuthorizationRequirement;
using OceansApp.Utility.Configuration.AuthorizationRequirement.AdminCenter;
using OceansApp.Utility.Configuration.AuthorizationRequirement.General;
using OceansApp.Utility.ConstantData.Claims.AdminCenter;
using OceansApp.Utility.ConstantData.Claims.Finances;
using OceansApp.Utility.ConstantData.Claims.General;
using OceansApp.Utility.ConstantData.Claims.Hours_TrackingTool;

namespace OceansApp.Utility.Configuration
{
    public static class AuthorizationConfig
    {
        public static void ConfigurePolicies(IServiceCollection services)
        {
            //ADMIN CENTER
            services.AddSingleton<IAuthorizationHandler, AnyOfPoliciesAdminCenterRequirementHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AnyOfPoliciesInAdminCenter", policy =>
                    policy.Requirements.Add(new AnyOfPoliciesAdminCenterRequirement()));
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToUpdateDataFromSoftlandSection", policy =>
                    policy.RequireClaim(AdminCenterClaimsCD.Actualizar_Datos_Softland_ClaimType, AdminCenterClaimsCD.Actualizar_Datos_Softland_ClaimValue));
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToUserAdministration", policy =>
                    policy.RequireClaim(AdminCenterClaimsCD.Administracion_Usuarios_ClaimType, AdminCenterClaimsCD.Administracion_Usuarios_ClaimValue));
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToUserRolesAndPermissions", policy =>
                    policy.RequireClaim(AdminCenterClaimsCD.Roles_Permisos_Usuarios_ClaimType, AdminCenterClaimsCD.Roles_Permisos_Usuarios_ClaimValue));
            });

            //FINANCES
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToAccountsReceivable", policy =>
                    policy.RequireClaim(FinancesClaimsCD.Accounts_Receivable_ClaimType, FinancesClaimsCD.Accounts_Receivable_ClaimValue));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToFinancialCalculator", policy =>
                    policy.RequireClaim(FinancesClaimsCD.Financial_Calculator_ClaimType, FinancesClaimsCD.Financial_Calculator_ClaimValue));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToFinancialCalculatorConfig", policy =>
                    policy.RequireClaim(FinancesClaimsCD.Financial_Calculator_BasicConfig_ClaimType, FinancesClaimsCD.Financial_Calculator_BasicConfig_ClaimValue));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToFinancialCalculatorAdvancedConfig", policy =>
                    policy.RequireClaim(FinancesClaimsCD.Financial_Calculator_AdvancedConfig_ClaimType, FinancesClaimsCD.Financial_Calculator_AdvancedConfig_ClaimValue));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToEditVacationsAndRemoveExpensesAndCostsFromFinancialCalculator", policy =>
                    policy.RequireClaim(FinancesClaimsCD.Financial_Calculator_Remove_Expenses_And_Costs_And_Edit_Vacations_ClaimType, FinancesClaimsCD.Financial_Calculator_Remove_Expenses_And_Costs_And_Edit_Vacations_ClaimValue));
            });

            //GENERAL - CONSULTANTS
            services.AddSingleton<IAuthorizationHandler, AnyOfPoliciesGeneralRequirementHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AnyOfPoliciesInGeneral", policy =>
                    policy.Requirements.Add(new AnyOfPoliciesGeneralRequirement()));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToConsultantsPage", policy =>
                    policy.RequireClaim(ConsultantsClaimsCD.Consultants_Page_ClaimType, ConsultantsClaimsCD.Consultants_Page_ClaimValue));
            });

            //HOURS TRACKING TOOL
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToTrackingTool", policy =>
                    policy.RequireClaim(HoursTrackingToolClaimsCD.Hours_Tracking_Tool_ClaimType, HoursTrackingToolClaimsCD.Hours_Tracking_Tool_ClaimValue));
            });



        }
    }
}
