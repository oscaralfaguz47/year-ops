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
            modelBuilder.Entity<ApplicationUser>()
               .HasOne(a => a.ApplicationUserCategory)
               .WithMany()
               .HasForeignKey(a => a.UserCategoryId)
               .IsRequired();

            modelBuilder.Entity<ApplicationUserClaim>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<ApplicationUserClaim>("ApplicationUser");

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
                .HasKey(rq => new { rq.ConsultantRoleId, rq.ConsultantQualityLevelId, rq.ConsultantSeniorityId });
            modelBuilder.Entity<ConsultantRolesQualityLevels>()
                .HasOne(rq => rq.ConsultantSeniority)
                .WithMany()
                .HasForeignKey(rq => rq.ConsultantSeniorityId)
                .IsRequired();

            //CONSULTANT_CLIENTS TABLE
            modelBuilder.Entity<ConsultantClient>()
                .HasKey(rq => new { rq.ConsultantId, rq.ClientId, rq.SuccessManager });
            modelBuilder.Entity<ConsultantClient>()
                .HasOne(cc => cc.ApplicationUser)
                .WithMany()
                .HasForeignKey(cc => cc.ConsultantId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ConsultantClient>()
                .HasOne(cc => cc.Client)
                .WithMany()
                .HasForeignKey(cc => cc.ClientId)
                .IsRequired();
            modelBuilder.Entity<ConsultantClient>()
                .HasOne(cc => cc.ApplicationUserSuccessManager)
                .WithMany()
                .HasForeignKey(cc => cc.SuccessManager)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ConsultantClient>()
                .HasOne(cc => cc.ApplicationUserCreate)
                .WithMany()
                .HasForeignKey(cc => cc.CreatedBy)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ConsultantClient>()
                .HasOne(cc => cc.ApplicationUserUpdate)
                .WithMany()
                .HasForeignKey(cc => cc.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            //CONSULTANT_CLIENTS TABLE END -------

            //CONSULTANT_DETAIL TABLE
            modelBuilder.Entity<ConsultantDetail>()
                .HasKey(rq => new { rq.ConsultantId});
            modelBuilder.Entity<ConsultantDetail>()
                .HasOne(cc => cc.ApplicationUser)
                .WithMany()
                .HasForeignKey(CC => CC.UserId)
                .IsRequired();
            modelBuilder.Entity<ConsultantDetail>()
                .HasOne(cc => cc.Country)
                .WithMany()
                .HasForeignKey(CC => CC.IdCountry)
                .IsRequired();

            modelBuilder.Entity<ConsultantHoliday>()
                .HasOne(cc => cc.ApplicationUser)
                .WithMany()
                .HasForeignKey(CC => CC.CreatedBy)
                .IsRequired();

            modelBuilder.Entity<ConsultantHolidayDate>()
                .HasOne(cc => cc.ConsultantHoliday)
                .WithMany()
                .HasForeignKey(CC => CC.ConsultantHolidayId)
                .IsRequired();

            modelBuilder.Entity<ConsultantHolidayDate>()
                .HasOne(cc => cc.ApplicationUserCreated)
                .WithMany()
                .HasForeignKey(CC => CC.CreatedBy)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConsultantHolidayDate>()
                .HasOne(cc => cc.ApplicationUserUpdated)
                .WithMany()
                .HasForeignKey(CC => CC.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DocumentsCCNotification>()
               .HasKey(d => new { d.DocumentCCId, d.NotificationId });

            modelBuilder.Entity<Client>()
                .Property(p => p.LatePaymentFee)
                .HasColumnType("decimal(18, 4)");

            // CONSULTANT AND POSITIONS
            modelBuilder.Entity<ConsultantAndPosition>()
                .HasKey(cp => new { cp.ConsultantId, cp.ConsultantPositionId });

            //PROJECTS
            modelBuilder.Entity<Project>()
                .HasKey(p => new { p.ProjectId });
            modelBuilder.Entity<Project>()
                .HasOne(p => p.ApplicationUserCreated)
                .WithMany()
                .HasForeignKey(p => p.CreatedBy)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Project>()
                .HasOne(p => p.ApplicationUserUpdated)
                .WithMany()
                .HasForeignKey(p => p.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Client)
                .WithMany()
                .HasForeignKey(p => p.ClientId)
                .IsRequired();
            modelBuilder.Entity<Project>()
                .HasOne(p => p.ConsultantDetail)
                .WithMany()
                .HasForeignKey(p => p.SuccessManagerId)
                .IsRequired();
            // PROJECTS CONSULTANTS ASSIGNED
            modelBuilder.Entity<ProjectConsultantAssigned>()
                .HasKey(p => new { p.ProjectConsultantAssignedId });
            modelBuilder.Entity<ProjectConsultantAssigned>()
                .HasOne(p => p.Project)
                .WithMany()
                .HasForeignKey(p => p.ProjectId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProjectConsultantAssigned>()
                .HasOne(cd => cd.ConsultantDetail)
                .WithMany()
                .HasForeignKey(cd => cd.ConsultantId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            // PROJECTS CONSULTANTS ASSIGNED HISTORY
            modelBuilder.Entity<ProjectConsultantAssignedHistory>()
                .HasNoKey();
            modelBuilder.Entity<ProjectConsultantAssignedHistory>()
                .HasOne(p => p.ProjectConsultantAssigned)
                .WithMany()
                .HasForeignKey(p => p.ProjectConsultantAssignedId)
                .IsRequired();
            modelBuilder.Entity<ProjectConsultantAssignedHistory>()
                .HasOne(a => a.ApplicationUserActionedBy)
                .WithMany()
                .HasForeignKey(a => a.UserActionedBy)
                .IsRequired();

        }
        public DbSet<AccountingAccount> ACCOUNTING_ACCOUNT { get; set; }
        public DbSet<CostCenter> COST_CENTER { get; set; }
        public DbSet<LedgerMovement> LEDGER_MOVEMENT { get; set; }
        public DbSet<DataUpdateDate> DATA_UPDATE_DATES { get; set; }
        public DbSet<ApplicationUser> AspNetUsers { get; set; }
        public DbSet<ApplicationUserCategory> UserCategories { get; set; }
        public DbSet<ApplicationRoleClaim> ApplicationRoleClaims { get; set; }
        public DbSet<ApplicationUserClaim> ApplicationUserClaims { get; set; }
        public DbSet<ApplicationSystemClaim> APPLICATION_SYSTEM_CLAIMS { get; set; }
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
        public DbSet<ConsultantPosition> CONSULTANT_POSITIONS { get; set; }
        public DbSet<ConsultantAndPosition> CONSULTANTS_AND_POSITIONS { get; set; }
        public DbSet<ConsultantClient> CONSULTANT_CLIENTS { get; set; }
        public DbSet<ConsultantDetail> CONSULTANT_DETAILS { get; set; }
        public DbSet<ConsultantHoliday> CONSULTANT_HOLIDAYS { get; set; }
        public DbSet<ConsultantHolidayDate> CONSULTANT_HOLIDAY_DATES { get; set; }
        public DbSet<ConsultantSeniority> CONSULTANT_SENIORITIS { get; set; }
        public DbSet<Project> PROJECTS { get; set; }
        public DbSet<ProjectConsultantAssigned> PROJECTS_CONSULTANTS_ASSIGNED { get; set; }
        public DbSet<ProjectConsultantAssignedHistory> PROJECTS_CONSULTANTS_ASSIGNED_HISTORY { get; set; }
        public DbSet<ProviderEvent> PROVIDER_EVENTS { get; set; }
        public DbSet<ProviderEventDate> PROVIDER_EVENT_DATES { get; set; }
        public DbSet<DocumentCC> DOCUMENTS_CC { get; set; }
        public DbSet<NotificationType> NOTIFICATION_TYPES { get; set; }
        public DbSet<Notification> NOTIFICATIONS { get; set; }
        public DbSet<NotificationStatus> NOTIFICATION_STATUS { get; set; }
        public DbSet<NotificationMedia> NOTIFICATION_MEDIA { get; set; }
        public DbSet<NotificationRecipient> NOTIFICATION_RECIPIENTS { get; set; }
        public DbSet<DocumentsCCNotification> DOCUMENTS_CC_NOTIFICATIONS { get; set; }
        public DbSet<SystemArea> SYSTEM_AREAS { get; set; }
        public DbSet<SystemSubArea> SYSTEM_SUB_AREAS { get; set; }
    }
}
