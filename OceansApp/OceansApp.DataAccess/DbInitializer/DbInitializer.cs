using OceansApp.Models.Models;
using OceansApp.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OceansApp.DataAccess.Data;
using OceansApp.Utility.ConstantData.Claims.AdminCenter;

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

                if (_db.SYSTEM_AREAS.ToList().Count == 0)
                {
                    List<SystemArea> systemAreasList = new List<SystemArea>();
                    systemAreasList.Add(new SystemArea() { Name = "Admin Center" });
                    systemAreasList.Add(new SystemArea() { Name = "Finanzas" });
                    systemAreasList.Add(new SystemArea() { Name = "General" });
                    systemAreasList.Add(new SystemArea() { Name = "Reporte de Horas" });
                    systemAreasList.Add(new SystemArea() { Name = "Dashboard" });
                    systemAreasList.Add(new SystemArea() { Name = "Mi Cuenta" });

                    foreach (var area in systemAreasList)
                    {
                        SystemArea sa = new()
                        {
                            Name = area.Name
                        };
                        _db.SYSTEM_AREAS.Add(sa);
                    }
                    _db.SaveChanges();
                }
                //Create System Sub Areaas

                if (_db.SYSTEM_SUB_AREAS.ToList().Count == 0)
                {
                    List<SystemSubArea> systemSubAreasList = new List<SystemSubArea>();
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 1, Name = "Actualizar Datos desde Softland" });
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 1, Name = "Administración de Usuarios" });
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 2, Name = "Cuentas Por Cobrar" });
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 2, Name = "Calculadora Financiera" });
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 3, Name = "Consultores" });
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 4, Name = "Reporte de Horas" });
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 5, Name = "Dashboard" });
                    systemSubAreasList.Add(new SystemSubArea() { SystemAreaId = 6, Name = "Mi Cuenta" });

                    foreach (var subArea in systemSubAreasList)
                    {
                        SystemSubArea ssa = new()
                        {
                            Name = subArea.Name,
                            SystemAreaId = subArea.SystemAreaId
                        };
                        _db.SYSTEM_SUB_AREAS.Add(ssa);
                    }
                    _db.SaveChanges();
                }
                //Create Claims

                if (_db.APPLICATION_SYSTEM_CLAIMS.ToList().Count == 0)
                {
                    var softlandSubAreaId = _db.SYSTEM_SUB_AREAS.FirstOrDefault(x => x.Name == "Actualizar Datos desde Softland");
                    List<ApplicationSystemClaim> systemClaimsList = new List<ApplicationSystemClaim>();
                    systemClaimsList.Add(new ApplicationSystemClaim() { ClaimType = "DatosSoftland", ClaimValue = "Have access to the update data from Softland section", 
                        Description = "Tiene acceso a poder actualizar los datos extraídos desde Softland", SystemSubAreaId =  softlandSubAreaId.SystemSubAreaId});

                    foreach (var claim in systemClaimsList)
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
                    _db.SaveChanges();
                }
            }
            return;
        }
    }
}
