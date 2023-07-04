using Microsoft.AspNetCore.Identity;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de ASP.NET Core Identity
            modelBuilder.Entity<IdentityUser>()
                .ToTable("Users")
                .HasKey(u => u.Id);

            modelBuilder.Entity<IdentityRole>()
                .ToTable("Roles")
                .HasKey(r => r.Id);

            // Configuración de la clave primaria para IdentityUserLogin<string>
            modelBuilder.Entity<IdentityUserLogin<string>>()
                .HasKey(login => new { login.LoginProvider, login.ProviderKey });

            // Configuración de la clave primaria para IdentityUserRole<string>
            modelBuilder.Entity<IdentityUserRole<string>>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // Configuración de la clave primaria para IdentityUserToken<string>
            modelBuilder.Entity<IdentityUserToken<string>>()
                .HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name });

            modelBuilder.Entity<ConsultantRolesQualityLevels>()
                .HasKey(rq => new {rq.ConsultantRoleId, rq.ConsultantQualityLevelId});
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
        public DbSet<ConsultantRole> CONSULTANT_ROLES { get; set; }
        public DbSet<ConsultantQualityLevel> CONSULTANT_QUALITY_LEVELS { get; set; }
        public DbSet<ConsultantRolesQualityLevels> CONSULTANT_ROLES_QUALITY_LEVELS { get; set; }
    }
}
