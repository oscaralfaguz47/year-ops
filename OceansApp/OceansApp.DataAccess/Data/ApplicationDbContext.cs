using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<AccountingAccount> ACCOUNTING_ACCOUNT { get; set; }
        public DbSet<CostCenter> COST_CENTER { get; set; }
        public DbSet<LedgerMovement> LEDGER_MOVEMENT { get; set; }
        public DbSet<DataUpdateDate> DATA_UPDATE_DATES { get; set; }
        public DbSet<ApplicationUser> AspNetUsers { get; set; }
        public DbSet<CalculatorGlobalConfiguration> CALCULATOR_GLOBAL_CONFIGURATIONS { get; set; }
        public DbSet<CalculatorCostCenterIncreaseConfiguration> CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS { get; set; }
        public DbSet<CalculatorSearchHistory> CALCULATOR_SEARCH_HISTORY { get; set; }
        public DbSet<CalculatorAccountingAccountToIgnore> CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE { get; set; }
        public DbSet<Client> CLIENT { get; set; }
        public DbSet<ProviderCategory> PROVIDER_CATEGORY { get; set; }
        public DbSet<Provider> PROVIDER { get; set; }
        public DbSet<Country> COUNTRY { get; set; }
    }
}
