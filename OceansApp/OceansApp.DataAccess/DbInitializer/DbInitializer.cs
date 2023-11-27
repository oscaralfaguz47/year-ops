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

            bool createDefaultDataToDatabase = true;

            if (createDefaultDataToDatabase)
            {
                //Create Roles if they are not created
                if (!_roleManager.RoleExistsAsync(SD.Role_User_Master).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Master)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Admin)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Simple)).GetAwaiter().GetResult();

                    //If Roles are not created, then we will create Master user as well
                    _userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = _config["MasterUserEmail"],
                        Email = _config["MasterUserEmail"],
                        Name = _config["MasterUserName"],
                        LastName = _config["MasterUserLastName"],
                        IsActive = true,
                        DeactivationDate = null
                    }, _config["MasterUserPass"]).GetAwaiter().GetResult();
                    ApplicationUser user = _db.AspNetUsers.FirstOrDefault(x => x.Email == _config["MasterUserEmail"]);

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
                //Create System Areaas
                    List<SystemArea> systemAreasList = new List<SystemArea>();
                    systemAreasList.Add(new SystemArea() { Name = "Admin Center" });
                    systemAreasList.Add(new SystemArea() { Name = "Finanzas" });
                    systemAreasList.Add(new SystemArea() { Name = "General" });
                    systemAreasList.Add(new SystemArea() { Name = "Reporte de Horas" });
                    systemAreasList.Add(new SystemArea() { Name = "Dashboard" });
                    systemAreasList.Add(new SystemArea() { Name = "Mi Cuenta" });

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

                //Create System Sub Areaas
                    List<SystemSubArea> systemSubAreasList = new List<SystemSubArea>();
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 1, Name = "Actualizar Datos desde Softland" });
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 1, Name = "Administración de Usuarios" });
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 1, Name = "Roles y Permisos de Usuarios" });
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 2, Name = "Cuentas Por Cobrar" });
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 2, Name = "Calculadora Financiera" });
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 3, Name = "Consultores" });
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 4, Name = "Herramienta de seguimiento de horas" });
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 5, Name = "Dashboard" });
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 6, Name = "Mi Cuenta" });

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
                    Description = "Acceso a la calculadora financiera",
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

                //HOURS TRACKING TOOL
                var hoursTrackingToolSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Herramienta de seguimiento de horas");
                systemClaimsList.Add(new ApplicationSystemClaim()
                {
                    ClaimType = HoursTrackingToolClaimsCD.Hours_Tracking_Tool_ClaimType,
                    ClaimValue = HoursTrackingToolClaimsCD.Hours_Tracking_Tool_ClaimValue,
                    Description = "Acceso a reportar horas en el tracking tool",
                    SystemSubAreaId = hoursTrackingToolSubAreaId.SystemSubAreaId
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
