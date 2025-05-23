using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OceansApp.Utility.Configuration.AuthorizationRequirement;
using OceansApp.Utility.Configuration.AuthorizationRequirement.AdminCenter;
using OceansApp.Utility.Configuration.AuthorizationRequirement.General;
using OceansApp.Utility.Configuration.AuthorizationRequirement.AccountManagement;
using OceansApp.Utility.ConstantData.Claims.AdminCenter;
using OceansApp.Utility.ConstantData.Claims.Finances;
using OceansApp.Utility.ConstantData.Claims.General;
using OceansApp.Utility.ConstantData.Claims.TrackingTool;
using OceansApp.Utility.ConstantData.Claims.AccountManagement;
using OceansApp.Utility.Configuration.AuthorizationRequirement.Finances;
using OceansApp.Utility.Configuration.AuthorizationRequirement.Recruiting;
using OceansApp.Utility.ConstantData.Claims.Recruiting;

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
                options.AddPolicy("AccessToUserRolesAndPermissions", policy =>
                    policy.RequireClaim(AdminCenterClaimsCD.Roles_Permisos_Usuarios_ClaimType, AdminCenterClaimsCD.Roles_Permisos_Usuarios_ClaimValue));
            });

            //ADMIN CENTER - CONSULTANT POSITIONS
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToConsultantPositions", policy =>
                    policy.RequireClaim(ConsultantPositionsClaimsCD.Manage_Consultant_Positions_ClaimType,
                    ConsultantPositionsClaimsCD.Manage_Consultant_Positions_ClaimValue));
            });

            //FINANCES
            services.AddSingleton<IAuthorizationHandler, AnyOfPoliciesFinancesRequirementHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AnyOfPoliciesInFinances", policy =>
                    policy.Requirements.Add(new AnyOfPoliciesFinancesRequirement()));
            });
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
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToManageConsultantPaymentsDebitsAndCredits", policy =>
                    policy.RequireClaim(FinancesClaimsCD.Manage_Payment_Debits_Credits_ClaimType, FinancesClaimsCD.Manage_Payment_Debits_Credits_ClaimValue));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToCostsCentersList", policy =>
                    policy.RequireClaim(FinancesClaimsCD.Manage_Payment_Debits_Credits_ClaimType, FinancesClaimsCD.Manage_Payment_Debits_Credits_ClaimValue));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToAccountingAccountsList", policy =>
                    policy.RequireClaim(FinancesClaimsCD.Manage_Payment_Debits_Credits_ClaimType, FinancesClaimsCD.Manage_Payment_Debits_Credits_ClaimValue));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToManageTheBasicsOfPaymentSheets", policy =>
                    policy.RequireClaim(FinancesClaimsCD.Manage_Basic_Payment_Sheets_ClaimType, FinancesClaimsCD.Manage_Basic_Payment_Sheets_ClaimValue));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToExportAccountingData", policy =>
                    policy.RequireClaim(FinancesClaimsCD.Access_Export_Accounting_Data_ClaimType, FinancesClaimsCD.Access_Export_Accounting_Data_ClaimValue));
            });
            //GENERAL
            services.AddSingleton<IAuthorizationHandler, AnyOfPoliciesGeneralRequirementHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AnyOfPoliciesInGeneral", policy =>
                    policy.Requirements.Add(new AnyOfPoliciesGeneralRequirement()));
            });
            //GENERAL - CONSULTANTS
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToConsultantsPage", policy =>
                    policy.RequireClaim(ConsultantsClaimsCD.Consultants_Page_ClaimType, ConsultantsClaimsCD.Consultants_Page_ClaimValue));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToManageAdministrativeConsultants", policy =>
                    policy.RequireClaim(ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimType, ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimValue));
            });
            //GENERAL - HOLIDAYS
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToHolidaysPage", policy =>
                    policy.RequireClaim(HolidaysClaimsCD.Holidays_Page_ClaimType, HolidaysClaimsCD.Holidays_Page_ClaimValue));
            });
            //GENERAL - CONSULTANT REIMBURSED BENEFITS
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToManageConsultantReimbursedBenefits", policy =>
                    policy.RequireClaim(ConsultantReimbursedBenefitsClaimsCD.Manage_Consultant_Reimbursed_Benefits_ClaimType,
                    ConsultantReimbursedBenefitsClaimsCD.Manage_Consultant_Reimbursed_Benefits_ClaimValue));
            });

            //PROJECT MANAGEMENT
            services.AddSingleton<IAuthorizationHandler, AnyOfPoliciesAccountManagementRequirementHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AnyOfPoliciesInAccountManagement", policy =>
                    policy.Requirements.Add(new AnyOfPoliciesAccountManagementRequirement()));
            });
            //PROJECT MANAGEMENT - CLIENTS
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToClientsPage", policy =>
                    policy.RequireClaim(ClientsClaimsCD.Clients_Page_ClaimType, ClientsClaimsCD.Clients_Page_ClaimValue));
            });

            //PROJECT MANAGEMENT - PROJECTS
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToProjectsPage", policy =>
                    policy.RequireClaim(ProjectsClaimsCD.Projects_Page_ClaimType, ProjectsClaimsCD.Projects_Page_ClaimValue));
            });

            //RECRUITING
            services.AddSingleton<IAuthorizationHandler, AnyOfPoliciesRecruitingRequirementHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AnyOfPoliciesInRecruiting", policy =>
                    policy.Requirements.Add(new AnyOfPoliciesRecruitingRequirement()));
            });
            //TRACKING TOOL - REPORTING MY TIME
            services.AddAuthorization(options =>
            {
                options.AddPolicy("BasicAccessToReportingMyTime", policy =>
                    policy.RequireClaim(TrackingToolClaimsCD.Reporting_My_Time_Basic_Access_ClaimType, TrackingToolClaimsCD.Reporting_My_Time_Basic_Access_ClaimValue));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToGeneralReports", policy =>
                    policy.RequireClaim(TrackingToolClaimsCD.General_Reports_ClaimType, TrackingToolClaimsCD.General_Reports_ClaimValue));
            });
            //INTERVIEWS
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToManageInterviews", policy =>
                    policy.RequireClaim(InterviewsClaimsCD.Manage_Interviews_Page_ClaimType, InterviewsClaimsCD.Manage_Interviews_ClaimValue));
            });

            //COMBINED POLICIES
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToSuccessManagersListForSelect", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ProjectsClaimsCD.Projects_Page_ClaimType
                        && claim.Value == ProjectsClaimsCD.Projects_Page_ClaimValue) ||
                        context.User.HasClaim(claim => claim.Type == ClientsClaimsCD.Clients_Page_ClaimType
                        && claim.Value == ClientsClaimsCD.Clients_Page_ClaimValue)));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToClientsListForSelect", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ProjectsClaimsCD.Projects_Page_ClaimType
                        && claim.Value == ProjectsClaimsCD.Projects_Page_ClaimValue)));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToSearchConsultantsBySearchText", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ProjectsClaimsCD.Projects_Page_ClaimType
                        && claim.Value == ProjectsClaimsCD.Projects_Page_ClaimValue)));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToSearchAllActiveConsultantsBySearchText", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ConsultantReimbursedBenefitsClaimsCD.Manage_Consultant_Reimbursed_Benefits_ClaimType
                        && claim.Value == ConsultantReimbursedBenefitsClaimsCD.Manage_Consultant_Reimbursed_Benefits_ClaimValue)
                        || context.User.HasClaim(claim => claim.Type == InterviewsClaimsCD.Manage_Interviews_Page_ClaimType
                        && claim.Value == InterviewsClaimsCD.Manage_Interviews_ClaimValue) || context.User.HasClaim(claim => claim.Type == FinancesClaimsCD.Accounts_Receivable_ClaimType
                        && claim.Value == FinancesClaimsCD.Accounts_Receivable_ClaimValue)));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToAllConsultantBenefitsListForSelect", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ConsultantReimbursedBenefitsClaimsCD.Manage_Consultant_Reimbursed_Benefits_ClaimType
                        && claim.Value == ConsultantReimbursedBenefitsClaimsCD.Manage_Consultant_Reimbursed_Benefits_ClaimValue)));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToGetSuccessManagerIdAndNameWhereClientId", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ProjectsClaimsCD.Projects_Page_ClaimType
                        && claim.Value == ProjectsClaimsCD.Projects_Page_ClaimValue)));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToViewNoSensitiveInfoForAllConsultants", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ConsultantsClaimsCD.Consultants_Page_ClaimType
                        && claim.Value == ConsultantsClaimsCD.Consultants_Page_ClaimValue) ||
                        context.User.HasClaim(claim => claim.Type == ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimType && claim.Value ==
                        ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimValue)));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToAllCountriesList", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ConsultantsClaimsCD.Consultants_Page_ClaimType
                        && claim.Value == ConsultantsClaimsCD.Consultants_Page_ClaimValue) ||
                        context.User.HasClaim(claim => claim.Type == ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimType && claim.Value ==
                        ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimValue)));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToAllCompaniesList", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ConsultantsClaimsCD.Consultants_Page_ClaimType
                        && claim.Value == ConsultantsClaimsCD.Consultants_Page_ClaimValue) ||
                        context.User.HasClaim(claim => claim.Type == ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimType && claim.Value ==
                        ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimValue) || context.User.HasClaim(claim => claim.Type == FinancesClaimsCD.Accounts_Receivable_ClaimType
                        && claim.Value == FinancesClaimsCD.Accounts_Receivable_ClaimValue)));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToAllDocumentTypesList", policy =>
                    policy.RequireAssertion(context => context.User.HasClaim(claim => claim.Type == FinancesClaimsCD.Accounts_Receivable_ClaimType
                        && claim.Value == FinancesClaimsCD.Accounts_Receivable_ClaimValue)));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToAllConsultantPositionsList", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ConsultantsClaimsCD.Consultants_Page_ClaimType
                        && claim.Value == ConsultantsClaimsCD.Consultants_Page_ClaimValue) ||
                        context.User.HasClaim(claim => claim.Type == ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimType && claim.Value ==
                        ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimValue) ||
                        context.User.HasClaim(claim => claim.Type == ProjectsClaimsCD.Projects_Page_ClaimType && claim.Value ==
                        ProjectsClaimsCD.Projects_Page_ClaimValue)));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToAllRolesList", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimType && claim.Value ==
                        ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimValue)));
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToPaymentMethodsList", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ConsultantsClaimsCD.Consultants_Page_ClaimType
                        && claim.Value == ConsultantsClaimsCD.Consultants_Page_ClaimValue) ||
                        context.User.HasClaim(claim => claim.Type == ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimType && claim.Value ==
                        ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimValue) ||
                        context.User.HasClaim(claim => claim.Type == FinancesClaimsCD.Manage_Basic_Payment_Sheets_ClaimType && claim.Value ==
                        FinancesClaimsCD.Manage_Basic_Payment_Sheets_ClaimValue)));
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToBankAccountsList", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == FinancesClaimsCD.Manage_Basic_Payment_Sheets_ClaimType && claim.Value ==
                        FinancesClaimsCD.Manage_Basic_Payment_Sheets_ClaimValue)));
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToListOfPartners", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ConsultantsClaimsCD.Consultants_Page_ClaimType
                        && claim.Value == ConsultantsClaimsCD.Consultants_Page_ClaimValue) ||
                        context.User.HasClaim(claim => claim.Type == ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimType && claim.Value ==
                        ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimValue)));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToListOfHolidaysForSelect", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ConsultantsClaimsCD.Consultants_Page_ClaimType
                        && claim.Value == ConsultantsClaimsCD.Consultants_Page_ClaimValue) ||
                        context.User.HasClaim(claim => claim.Type == ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimType && claim.Value ==
                        ConsultantsClaimsCD.Manage_Administrative_Consultants_ClaimValue)));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToActiveProjectsListForSelect", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == FinancesClaimsCD.Manage_Basic_Payment_Sheets_ClaimType
                        && claim.Value == FinancesClaimsCD.Manage_Basic_Payment_Sheets_ClaimValue)));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToAllTransactionStatusesList", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ConsultantReimbursedBenefitsClaimsCD.Manage_Consultant_Reimbursed_Benefits_ClaimType
                        && claim.Value == ConsultantReimbursedBenefitsClaimsCD.Manage_Consultant_Reimbursed_Benefits_ClaimValue)));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToSearchForProjectsList", policy =>
                    policy.RequireClaim(FinancesClaimsCD.Accounts_Receivable_ClaimType, FinancesClaimsCD.Accounts_Receivable_ClaimValue));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AccessToCreateAndUpdateProducts", policy =>
                    policy.RequireClaim(FinancesClaimsCD.Accounts_Receivable_ClaimType, FinancesClaimsCD.Accounts_Receivable_ClaimValue));
            });

        }
    }
}
