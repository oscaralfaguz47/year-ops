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

            //Create Default Consultant Roles

            if (_db.CONSULTANT_ROLES.ToList().Count == 0)
            {
                List<ConsultantRole> consultantRolesList = new List<ConsultantRole>();
                consultantRolesList.Add(new ConsultantRole() { Name = "Developer" });
                consultantRolesList.Add(new ConsultantRole() { Name = "Quality Assurance" });

                foreach (var role in consultantRolesList)
                {
                    ConsultantRole consultantRole = new()
                    {
                        Name = role.Name
                    };
                    _db.CONSULTANT_ROLES.Add(consultantRole);
                }
                _db.SaveChanges();
            }

            //Create Default Consultant Quality Levels

            if (_db.CONSULTANT_QUALITY_LEVELS.ToList().Count == 0)
            {
                List<ConsultantQualityLevel> qualityLevelsList = new List<ConsultantQualityLevel>();
                qualityLevelsList.Add(new ConsultantQualityLevel() { Name = "Lead" });
                qualityLevelsList.Add(new ConsultantQualityLevel() { Name = "AAA" });
                qualityLevelsList.Add(new ConsultantQualityLevel() { Name = "AA" });
                qualityLevelsList.Add(new ConsultantQualityLevel() { Name = "A" });

                foreach (var qualityLevel in qualityLevelsList)
                {
                    ConsultantQualityLevel cQualityLeverl = new()
                    {
                        Name = qualityLevel.Name
                    };
                    _db.CONSULTANT_QUALITY_LEVELS.Add(cQualityLeverl);
                }
                _db.SaveChanges();
            }

            //Create Default Consultant Roles Quality Levels

            if (_db.CONSULTANT_ROLES_QUALITY_LEVELS.ToList().Count == 0)
            {
                List<ConsultantRolesQualityLevels> rolesQualityLevelsList = new List<ConsultantRolesQualityLevels>();
                var developerRole = _db.CONSULTANT_ROLES.FirstOrDefault(x=> x.Name == "Developer");
                var qaRole = _db.CONSULTANT_ROLES.FirstOrDefault(x => x.Name == "Quality Assurance");
                var leadLevel = _db.CONSULTANT_QUALITY_LEVELS.FirstOrDefault(x => x.Name == "Lead");
                var aaaLevel = _db.CONSULTANT_QUALITY_LEVELS.FirstOrDefault(x => x.Name == "AAA");
                var aaLevel = _db.CONSULTANT_QUALITY_LEVELS.FirstOrDefault(x => x.Name == "AA");
                var aLevel = _db.CONSULTANT_QUALITY_LEVELS.FirstOrDefault(x => x.Name == "A");

                if (developerRole != null && qaRole != null && leadLevel != null && aaaLevel != null && aaLevel != null && aLevel != null)
                {
                    rolesQualityLevelsList.Add(new ConsultantRolesQualityLevels()
                    {
                        ConsultantRoleId = developerRole.ConsultantRoleId,
                        ConsultantQualityLevelId = leadLevel.ConsultantQualityLevelId,
                        ClientRateMaximumAmount = 65,
                        ConsultantMaximumAmount = 7500,
                        UpdatedDate = DateTime.Now
                    });

                    rolesQualityLevelsList.Add(new ConsultantRolesQualityLevels()
                    {
                        ConsultantRoleId = developerRole.ConsultantRoleId,
                        ConsultantQualityLevelId = aaaLevel.ConsultantQualityLevelId,
                        ClientRateMaximumAmount = 60,
                        ConsultantMaximumAmount = 6500,
                        UpdatedDate = DateTime.Now
                    });

                    rolesQualityLevelsList.Add(new ConsultantRolesQualityLevels()
                    {
                        ConsultantRoleId = developerRole.ConsultantRoleId,
                        ConsultantQualityLevelId = aaLevel.ConsultantQualityLevelId,
                        ClientRateMaximumAmount = 55,
                        ConsultantMaximumAmount = 5500,
                        UpdatedDate = DateTime.Now
                    });

                    rolesQualityLevelsList.Add(new ConsultantRolesQualityLevels()
                    {
                        ConsultantRoleId = developerRole.ConsultantRoleId,
                        ConsultantQualityLevelId = aLevel.ConsultantQualityLevelId,
                        ClientRateMaximumAmount = 50,
                        ConsultantMaximumAmount = 4500,
                        UpdatedDate = DateTime.Now
                    });

                    rolesQualityLevelsList.Add(new ConsultantRolesQualityLevels()
                    {
                        ConsultantRoleId = qaRole.ConsultantRoleId,
                        ConsultantQualityLevelId = leadLevel.ConsultantQualityLevelId,
                        ClientRateMaximumAmount = 60,
                        ConsultantMaximumAmount = 6000,
                        UpdatedDate = DateTime.Now
                    });

                    rolesQualityLevelsList.Add(new ConsultantRolesQualityLevels()
                    {
                        ConsultantRoleId = qaRole.ConsultantRoleId,
                        ConsultantQualityLevelId = aaaLevel.ConsultantQualityLevelId,
                        ClientRateMaximumAmount = 55,
                        ConsultantMaximumAmount = 5000,
                        UpdatedDate = DateTime.Now
                    });

                    rolesQualityLevelsList.Add(new ConsultantRolesQualityLevels()
                    {
                        ConsultantRoleId = qaRole.ConsultantRoleId,
                        ConsultantQualityLevelId = aaLevel.ConsultantQualityLevelId,
                        ClientRateMaximumAmount = 50,
                        ConsultantMaximumAmount = 4000,
                        UpdatedDate = DateTime.Now
                    });

                    rolesQualityLevelsList.Add(new ConsultantRolesQualityLevels()
                    {
                        ConsultantRoleId = qaRole.ConsultantRoleId,
                        ConsultantQualityLevelId = aLevel.ConsultantQualityLevelId,
                        ClientRateMaximumAmount = 40,
                        ConsultantMaximumAmount = 3000,
                        UpdatedDate = DateTime.Now
                    });

                    foreach (var roleLevel in rolesQualityLevelsList)
                    {
                        ConsultantRolesQualityLevels cRoleLevel = new()
                        {
                            ConsultantRoleId = roleLevel.ConsultantRoleId,
                            ConsultantQualityLevelId = roleLevel.ConsultantQualityLevelId,
                            ClientRateMaximumAmount = roleLevel.ClientRateMaximumAmount,
                            ConsultantMaximumAmount = roleLevel.ConsultantMaximumAmount,
                            UpdatedDate = roleLevel.UpdatedDate
                        };
                        _db.CONSULTANT_ROLES_QUALITY_LEVELS.Add(cRoleLevel);
                    }
                    _db.SaveChanges();
                }
            }

            return;
        }
    }
}
