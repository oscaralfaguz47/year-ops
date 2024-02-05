using OceansApp.Models.Models;
using OceansApp.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OceansApp.DataAccess.Data;
using OceansApp.Utility.ConstantData.Claims.AdminCenter;
using OceansApp.Utility.ConstantData.Claims.Finances;
using OceansApp.Utility.ConstantData.Claims.General;
using OceansApp.Utility.ConstantData.Claims.Hours_TrackingTool;
using OceansApp.Utility.ConstantData.Claims.AccountManagement;

namespace OceansApp.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        public IConfiguration _config { get; }
        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db,
            IConfiguration config)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
            _config = config;
        }

        public void Initialize()
        {
            bool isThereNewMigrationToUpdate = false; // False if no migration updates in the DB are needed
            if (isThereNewMigrationToUpdate)
            {
                //Migrations if they are not applied
                try
                {
                    if (_db.Database.GetPendingMigrations().Count() > 0)
                    {
                        _db.Database.Migrate();
                    }
                }
                catch (Exception ex)
                {

                }
            }

            bool createDefaultDataToDatabase = false; // False if no updates in the DB are needed

            if (createDefaultDataToDatabase)
            {
                //Create Default roles
                List<IdentityRole> rolesList = new List<IdentityRole>();
                rolesList.Add(new IdentityRole() { Name = SD.Role_User_Master });
                rolesList.Add(new IdentityRole() { Name = SD.Role_User_Admin });
                rolesList.Add(new IdentityRole() { Name = SD.Role_User_Simple });
                rolesList.Add(new IdentityRole() { Name = SD.Role_User_Create_Consultants });

                //Create Default User Categories
                List<ApplicationUserCategory> userCategoriesList = new List<ApplicationUserCategory>();
                userCategoriesList.Add(new ApplicationUserCategory() { Name = "Administrative" });
                userCategoriesList.Add(new ApplicationUserCategory() { Name = "Consultant" });
                userCategoriesList.Add(new ApplicationUserCategory() { Name = "External User" });
                foreach (var userCategory in userCategoriesList)
                {
                    if (_db.UserCategories.FirstOrDefault(x => x.Name == userCategory.Name) == null)
                    {
                        _db.UserCategories.Add(userCategory);
                    }
                    _db.SaveChanges();
                }

                foreach (var role in rolesList)
                {
                    if (_roleManager.FindByNameAsync(role.Name).Result == null)
                    {
                        _roleManager.CreateAsync(new IdentityRole(role.Name)).GetAwaiter().GetResult();
                    }
                }
                //Create user manager if it is not created
                if (!_roleManager.RoleExistsAsync(SD.Role_User_Master).GetAwaiter().GetResult())
                {
                    //If Roles are not created, then we will create Master user as well
                    _userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = Environment.GetEnvironmentVariable(_config["MasterUserEmail"]),
                        Email = Environment.GetEnvironmentVariable(_config["MasterUserEmail"]),
                        Name = _config["MasterUserName"],
                        LastName = _config["MasterUserLastName"],
                        IsActive = true,
                        DeactivationDate = null
                    }, Environment.GetEnvironmentVariable(_config["MasterUserPass_ENV"])).GetAwaiter().GetResult();
                    ApplicationUser user = _db.AspNetUsers.FirstOrDefault(x => x.Email == Environment.GetEnvironmentVariable(_config["MasterUserEmail"]));

                    _userManager.AddToRoleAsync(user, SD.Role_User_Master).GetAwaiter().GetResult();
                }

                //Create Default Provider Events

                if (_db.PROVIDER_EVENTS.ToList().Count == 0)
                {
                    List<ProviderEvent> providerEventsList = new List<ProviderEvent>();
                    providerEventsList.Add(new ProviderEvent() { Name = "Entrada" });
                    providerEventsList.Add(new ProviderEvent() { Name = "Salida" });
                    providerEventsList.Add(new ProviderEvent() { Name = "Contrato Firmado por 1era vez" });
                    providerEventsList.Add(new ProviderEvent() { Name = "Contrato actualizado" });

                    foreach (var pEvent in providerEventsList)
                    {
                        ProviderEvent providerEvent = new()
                        {
                            Name = pEvent.Name
                        };
                        _db.PROVIDER_EVENTS.Add(providerEvent);
                    }
                    _db.SaveChanges();
                }

                //Create Default Notifications stuff

                if (_db.NOTIFICATION_MEDIA.ToList().Count == 0)
                {
                    List<NotificationMedia> notificatinMediaList = new List<NotificationMedia>();
                    notificatinMediaList.Add(new NotificationMedia() { Name = "Email" });
                    notificatinMediaList.Add(new NotificationMedia() { Name = "Slack" });

                    foreach (var notMedia in notificatinMediaList)
                    {
                        NotificationMedia notificationMedia = new()
                        {
                            Name = notMedia.Name
                        };
                        _db.NOTIFICATION_MEDIA.Add(notificationMedia);
                    }
                    _db.SaveChanges();
                }
                if (_db.NOTIFICATION_STATUS.ToList().Count == 0)
                {
                    List<NotificationStatus> notificatinStatusList = new List<NotificationStatus>();
                    notificatinStatusList.Add(new NotificationStatus() { Name = "Enviado" });
                    notificatinStatusList.Add(new NotificationStatus() { Name = "No enviado" });
                    notificatinStatusList.Add(new NotificationStatus() { Name = "Envío fallido" });
                    foreach (var notStatus in notificatinStatusList)
                    {
                        NotificationStatus notificationStatus = new()
                        {
                            Name = notStatus.Name
                        };
                        _db.NOTIFICATION_STATUS.Add(notificationStatus);
                    }
                    _db.SaveChanges();
                }
                if (_db.NOTIFICATION_TYPES.ToList().Count == 0)
                {
                    List<NotificationType> notificationTypeList = new List<NotificationType>();
                    notificationTypeList.Add(new NotificationType() { Name = "Cuentas por cobrar" });
                    foreach (var notType in notificationTypeList)
                    {
                        NotificationType notificationType = new()
                        {
                            Name = notType.Name
                        };
                        _db.NOTIFICATION_TYPES.Add(notificationType);
                    }
                    _db.SaveChanges();
                }

                if (_db.CONSULTANT_POSITIONS.ToList().Count == 0)
                {
                    List<ConsultantPosition> positionsList = new List<ConsultantPosition>();
                    positionsList.Add(new ConsultantPosition() { Name = "Success Manager", IsAdministrative = true });
                    positionsList.Add(new ConsultantPosition() { Name = "CEO", IsAdministrative = true });
                    positionsList.Add(new ConsultantPosition() { Name = "CFO", IsAdministrative = true });
                    positionsList.Add(new ConsultantPosition() { Name = "Recruiting Manager", IsAdministrative = true });
                    positionsList.Add(new ConsultantPosition() { Name = "Strategy Director", IsAdministrative = true });
                    positionsList.Add(new ConsultantPosition() { Name = "Recruiter", IsAdministrative = true });
                    positionsList.Add(new ConsultantPosition() { Name = "Marketing Manager", IsAdministrative = true });
                    positionsList.Add(new ConsultantPosition() { Name = "People and Culture", IsAdministrative = true });
                    positionsList.Add(new ConsultantPosition() { Name = "Gifts Coordinator", IsAdministrative = true });
                    positionsList.Add(new ConsultantPosition() { Name = "Junior Sales Executive", IsAdministrative = true });
                    positionsList.Add(new ConsultantPosition() { Name = "Sales Executive", IsAdministrative = true });
                    positionsList.Add(new ConsultantPosition() { Name = "Payment Assistant", IsAdministrative = true });
                    positionsList.Add(new ConsultantPosition() { Name = "Financial Assistant", IsAdministrative = true });
                    positionsList.Add(new ConsultantPosition() { Name = "Full Stack Developer IT Support", IsAdministrative = true });

                    positionsList.Add(new ConsultantPosition() { Name = "Senior Developer", IsAdministrative = false });
                    positionsList.Add(new ConsultantPosition() { Name = "Full Stack Developer", IsAdministrative = false });
                    positionsList.Add(new ConsultantPosition() { Name = "Data Engineer", IsAdministrative = false });
                    positionsList.Add(new ConsultantPosition() { Name = "Senior QA Engineer", IsAdministrative = false });
                    positionsList.Add(new ConsultantPosition() { Name = "Mid Developer", IsAdministrative = false });
                    positionsList.Add(new ConsultantPosition() { Name = "Project Manager", IsAdministrative = false });
                    positionsList.Add(new ConsultantPosition() { Name = "Team Lead", IsAdministrative = false });
                    positionsList.Add(new ConsultantPosition() { Name = "AWS Engineer", IsAdministrative = false });
                    positionsList.Add(new ConsultantPosition() { Name = "DevOps Engineer", IsAdministrative = false });
                    positionsList.Add(new ConsultantPosition() { Name = "SRE Developer", IsAdministrative = false });
                    positionsList.Add(new ConsultantPosition() { Name = "Mobile Developer", IsAdministrative = false });
                    positionsList.Add(new ConsultantPosition() { Name = "QA Lead", IsAdministrative = false });

                    foreach (var conPosition in positionsList)
                    {
                        ConsultantPosition consultantPosition = new()
                        {
                            Name = conPosition.Name,
                            IsAdministrative = conPosition.IsAdministrative
                        };
                        _db.CONSULTANT_POSITIONS.Add(consultantPosition);
                    }
                    _db.SaveChanges();
                }
                //CREATE PROJECT HISTORY ACTIONS
                if (_db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS.ToList().Count == 0)
                {
                    List<ProjectConsultantAssignedHistoryAction> actionsList = new();
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Consultant Assigned First Time" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Position Details updated" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Hourly Client Rate updated" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Monthly Client Rate updated" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Hourly Salary updated" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Monthly Salary updated" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Client pricing method updated (Monthly)" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Client pricing method updated (Hourly)" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Consultant pricing method updated (Monthly)" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Consultant pricing method updated (Hourly)" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Consultant Activated" });
                    actionsList.Add(new ProjectConsultantAssignedHistoryAction() { Name = "Consultant Deactivated" });
                    foreach (var action in actionsList)
                    {
                        ProjectConsultantAssignedHistoryAction actionToSave = new()
                        {
                            Name = action.Name
                        };
                        _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS.Add(actionToSave);
                    }
                    _db.SaveChanges();
                }

                //Create System Areas
                List<SystemArea> systemAreasList = new List<SystemArea>();
                systemAreasList.Add(new SystemArea() { Name = "Admin Center" });
                systemAreasList.Add(new SystemArea() { Name = "Finanzas" });
                systemAreasList.Add(new SystemArea() { Name = "General" });
                systemAreasList.Add(new SystemArea() { Name = "Reporte de Horas" });
                systemAreasList.Add(new SystemArea() { Name = "Dashboard" });
                systemAreasList.Add(new SystemArea() { Name = "Mi Cuenta" });
                systemAreasList.Add(new SystemArea() { Name = "Account Management" });

                foreach (var area in systemAreasList)
                {
                    if (_db.SYSTEM_AREAS.FirstOrDefault(x => x.Name == area.Name) == null)
                    {
                        SystemArea sa = new()
                        {
                            Name = area.Name
                        };
                        _db.SYSTEM_AREAS.Add(sa);
                    }
                }
                _db.SaveChanges();

                //Create System Sub Areas
                List<SystemSubArea> systemSubAreasList = new List<SystemSubArea>();
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 1, Name = "Actualizar Datos desde Softland" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 1, Name = "Administración de Usuarios" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 1, Name = "Roles y Permisos de Usuarios" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 2, Name = "Cuentas Por Cobrar" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 2, Name = "Calculadora Financiera" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 3, Name = "Consultores" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 3, Name = "Holidays" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 4, Name = "Herramienta de seguimiento de horas" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 5, Name = "Dashboard" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 6, Name = "Mi Cuenta" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 7, Name = "Clients" });
                systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 7, Name = "Projects" });

                foreach (var subArea in systemSubAreasList)
                {
                    if (_db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == subArea.Name) == null)
                    {
                        SystemSubArea ssa = new()
                        {
                            Name = subArea.Name,
                            SystemAreaId = subArea.SystemAreaId
                        };
                        _db.SYSTEM_SUB_AREAS.Add(ssa);
                    }
                }
                _db.SaveChanges();


                //Create Claims


                List<ApplicationSystemClaim> systemClaimsList = new List<ApplicationSystemClaim>();

                //ADMIN CENTER
                var softlandSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Actualizar Datos desde Softland");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = AdminCenterClaimsCD.Actualizar_Datos_Softland_ClaimType,
                    ClaimValue = AdminCenterClaimsCD.Actualizar_Datos_Softland_ClaimValue,
                    Description = "Acceso a poder actualizar los datos extraídos desde Softland",
                    SystemSubAreaId = softlandSubAreaId.SystemSubAreaId
                });

                var adminUserSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Administración de Usuarios");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = AdminCenterClaimsCD.Administracion_Usuarios_ClaimType,
                    ClaimValue = AdminCenterClaimsCD.Administracion_Usuarios_ClaimValue,
                    Description = "Acceso a ver todos los usuarios del sistema",
                    SystemSubAreaId = adminUserSubAreaId.SystemSubAreaId
                });

                var userRolesPermissionsSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Roles y Permisos de Usuarios");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = AdminCenterClaimsCD.Roles_Permisos_Usuarios_ClaimType,
                    ClaimValue = AdminCenterClaimsCD.Roles_Permisos_Usuarios_ClaimValue,
                    Description = "Acceso a ver y editar los roles y permisos de usuarios",
                    SystemSubAreaId = userRolesPermissionsSubAreaId.SystemSubAreaId
                });
                // NOTES FOR ADMIN CENTER PERMISSIONS:
                // Add every permission to the AnyOfPoliciesAdminCenterRequirementHandler

                //FINANCES
                var accountsReceivableSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Cuentas Por Cobrar");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = FinancesClaimsCD.Accounts_Receivable_ClaimType,
                    ClaimValue = FinancesClaimsCD.Accounts_Receivable_ClaimValue,
                    Description = "Acceso a la sección de cuentas por cobrar",
                    SystemSubAreaId = accountsReceivableSubAreaId.SystemSubAreaId
                });

                var financialCalculatorSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Calculadora Financiera");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = FinancesClaimsCD.Financial_Calculator_ClaimType,
                    ClaimValue = FinancesClaimsCD.Financial_Calculator_ClaimValue,
                    Description = "Acceso básico a la calculadora financiera",
                    SystemSubAreaId = financialCalculatorSubAreaId.SystemSubAreaId
                });
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = FinancesClaimsCD.Financial_Calculator_BasicConfig_ClaimType,
                    ClaimValue = FinancesClaimsCD.Financial_Calculator_BasicConfig_ClaimValue,
                    Description = "Acceso a la configuración básica de la calculadora financiera",
                    SystemSubAreaId = financialCalculatorSubAreaId.SystemSubAreaId
                });
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = FinancesClaimsCD.Financial_Calculator_AdvancedConfig_ClaimType,
                    ClaimValue = FinancesClaimsCD.Financial_Calculator_AdvancedConfig_ClaimValue,
                    Description = "Acceso avanzado en la configuración de la calculadora (editar los porcentages de utilidad, porcentages de riesgo, etc.)",
                    SystemSubAreaId = financialCalculatorSubAreaId.SystemSubAreaId
                });
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = FinancesClaimsCD.Financial_Calculator_Profit_And_Details_ClaimType,
                    ClaimValue = FinancesClaimsCD.Financial_Calculator_Profit_And_Details_ClaimValue,
                    Description = "Acceso a ver las utilidades de los resultados de la calculadora financiera y a más detalles.",
                    SystemSubAreaId = financialCalculatorSubAreaId.SystemSubAreaId
                });
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = FinancesClaimsCD.Financial_Calculator_Remove_Expenses_And_Costs_And_Edit_Vacations_ClaimType,
                    ClaimValue = FinancesClaimsCD.Financial_Calculator_Remove_Expenses_And_Costs_And_Edit_Vacations_ClaimValue,
                    Description = "Acceso a editar la opcion de vacaciones y remover gastos y costos para no ser tomados en cuenta en el calculo de la calculadora financiera.",
                    SystemSubAreaId = financialCalculatorSubAreaId.SystemSubAreaId
                });

                //GENERAL - CONSULTANTS
                var consultantsSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Consultores");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = ConsultantsClaimsCD.Consultants_Page_ClaimType,
                    ClaimValue = ConsultantsClaimsCD.Consultants_Page_ClaimValue,
                    Description = "Acceso para ver a todos los consultores",
                    SystemSubAreaId = consultantsSubAreaId.SystemSubAreaId
                });

                //GENERAL - HOLIDAYS
                var holidaysSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Holidays");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = HolidaysClaimsCD.Holidays_Page_ClaimType,
                    ClaimValue = HolidaysClaimsCD.Holidays_Page_ClaimValue,
                    Description = "Acceso básico para ver todos los holidays",
                    SystemSubAreaId = holidaysSubAreaId.SystemSubAreaId
                });

                //HOURS TRACKING TOOL
                var hoursTrackingToolSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Herramienta de seguimiento de horas");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = HoursTrackingToolClaimsCD.Hours_Tracking_Tool_ClaimType,
                    ClaimValue = HoursTrackingToolClaimsCD.Hours_Tracking_Tool_ClaimValue,
                    Description = "Acceso a reportar horas en el tracking tool",
                    SystemSubAreaId = hoursTrackingToolSubAreaId.SystemSubAreaId
                });

                //PROJECT MANAGEMENT - CLIENTS
                var clientsSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Clients");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = ClientsClaimsCD.Clients_Page_ClaimType,
                    ClaimValue = ClientsClaimsCD.Clients_Page_ClaimValue,
                    Description = "Acces to view the Clients list",
                    SystemSubAreaId = clientsSubAreaId.SystemSubAreaId
                });
                //PROJECT MANAGEMENT - PROJECTS
                var projectsSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Projects");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = ProjectsClaimsCD.Projects_Page_ClaimType,
                    ClaimValue = ProjectsClaimsCD.Projects_Page_ClaimValue,
                    Description = "Acces to view the Projects list",
                    SystemSubAreaId = projectsSubAreaId.SystemSubAreaId
                });

                foreach (var claim in systemClaimsList)
                {
                    if (_db.APPLICATION_SYSTEM_CLAIMS.FirstOrDefault(x => x.ClaimType == claim.ClaimType && x.ClaimValue == claim.ClaimValue) == null)
                    {
                        ApplicationSystemClaim asc = new()
                        {
                            ClaimType = claim.ClaimType,
                            ClaimValue = claim.ClaimValue,
                            Description = claim.Description,
                            SystemSubAreaId = claim.SystemSubAreaId
                        };
                        _db.APPLICATION_SYSTEM_CLAIMS.Add(asc);
                    }
                }
                _db.SaveChanges();
            }
            return;
        }
    }
}
