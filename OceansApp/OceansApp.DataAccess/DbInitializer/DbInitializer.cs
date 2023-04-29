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
            //Create Accounting Accounts to Ignore

            if (_db.CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE.ToList().Count == 0)
            {
                List<CalculatorAccountingAccountToIgnore> accountsList = new List<CalculatorAccountingAccountToIgnore>();
                accountsList.Add(new CalculatorAccountingAccountToIgnore() { IdAccountingAccount = "5-01-01-000-000", ExpenseType = "Sales Cost" });
                accountsList.Add(new CalculatorAccountingAccountToIgnore() { IdAccountingAccount = "5-01-02-000-000", ExpenseType = "Sales Cost" });
                accountsList.Add(new CalculatorAccountingAccountToIgnore() { IdAccountingAccount = "5-01-06-000-000", ExpenseType = "Sales Cost" });
                accountsList.Add(new CalculatorAccountingAccountToIgnore() { IdAccountingAccount = "5-01-22-000-000", ExpenseType = "Sales Cost" });
                accountsList.Add(new CalculatorAccountingAccountToIgnore() { IdAccountingAccount = "5-01-16-000-000", ExpenseType = "Sales Cost" });
                foreach (var account in accountsList)
                {
                    CalculatorAccountingAccountToIgnore accountingAccountToIgnore = new()
                    {
                        IdAccountingAccount = account.IdAccountingAccount,
                        ExpenseType = account.ExpenseType
                    };
                    _db.CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE.Add(accountingAccountToIgnore);
                }
                _db.SaveChanges();
            }

            return;
        }
    }
}
