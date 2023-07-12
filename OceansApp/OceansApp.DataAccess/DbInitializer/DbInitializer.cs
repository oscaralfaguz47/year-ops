using OceansApp.Models.Models;
using OceansApp.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OceansApp.DataAccess.Data;

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

            return;
        }
    }
}
