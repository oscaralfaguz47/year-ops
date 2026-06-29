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
            modelBuilder.Entity<IdentityUser>(entity =>
            {
                entity.ToTable("Users")
                .HasKey(u => u.Id);
            });

            // ACCOUNTING ACCOUNTS
            modelBuilder.Entity<AccountingAccount>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.AccountingAccountId, e.Description });

                entity.HasIndex(e => e.AccountingAccountId);
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.AccountingAccountCode);
            });

            // ACCOUNTS PAYABLE
            modelBuilder.Entity<AccountPayable>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.ConsultantId, e.StartDatePeriod, e.EndDatePeriod });
                entity.HasIndex(e => new { e.ConsultantId, e.TransactionStatusId, e.StartDatePeriod, e.EndDatePeriod });
                entity.HasIndex(e => new { e.StartDatePeriod, e.EndDatePeriod });

                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.UserCreatedBy);
                entity.HasIndex(e => e.UserLastUpdatedBy);
                entity.HasIndex(e => e.AccountingDate);
                entity.HasIndex(e => e.TransactionStatusId);
                entity.HasIndex(e => e.BalanceAmount);
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.Voided);

                entity.HasOne(cp => cp.ConsultantDetail)
                .WithMany()
                .HasForeignKey(cp => cp.ConsultantId)
                .IsRequired();
                entity.HasOne(p => p.ApplicationUserCreatedBy)
              .WithMany()
              .HasForeignKey(p => p.UserCreatedBy)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.ApplicationUserUpdatedBy)
              .WithMany()
              .HasForeignKey(p => p.UserLastUpdatedBy)
              .OnDelete(DeleteBehavior.Restrict);
                entity.Property(d => d.AccountingDate)
                 .HasColumnType("date")
                 .IsRequired();
                entity.Property(d => d.StartDatePeriod)
                 .HasColumnType("date")
                 .IsRequired();
                entity.Property(d => d.EndDatePeriod)
                 .HasColumnType("date")
                 .IsRequired();
                entity.Property(c => c.CompanyId)
                 .HasColumnType("varchar(8)");
                entity.HasOne(p => p.Company)
               .WithMany()
               .HasForeignKey(p => p.CompanyId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.TransactionStatus)
              .WithMany()
              .HasForeignKey(p => p.TransactionStatusId)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
                entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)");
                entity.Property(e => e.BalanceAmount)
                .HasColumnType("decimal(18, 2)");
            });

            // ACCOUNTS PAYABLE MOVEMENTS
            modelBuilder.Entity<AccountPayableMovement>(entity =>
            {
                entity.HasIndex(e => e.AccountPayableId);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.MovementTypeId);

                entity.HasKey(c => new { c.Id });
                entity.HasOne(cp => cp.Project)
                .WithMany()
                .HasForeignKey(cp => cp.ProjectId);
                entity.HasOne(p => p.MovementType)
              .WithMany()
              .HasForeignKey(p => p.MovementTypeId)
              .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.AccountPayable)
              .WithMany()
              .HasForeignKey(p => p.AccountPayableId)
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();
                entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(18, 6)");
            });

            // APPLICATION USER
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.Id, e.Name, e.LastName });
                entity.HasIndex(e => new { e.Name, e.LastName });
                entity.HasIndex(e => e.Id)
        .HasDatabaseName("IX_Users_Id")
        .IncludeProperties(e => new { e.Name, e.LastName });

                // Indexes on foreign keys
                entity.HasIndex(e => e.UserCategoryId);

                // Indexes for columns
                entity.HasIndex(e => e.Id);
                entity.HasIndex(e => e.Occupation);
                entity.HasIndex(e => e.DeactivationDate);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.EmailConfirmed);
                entity.HasIndex(e => e.PhoneNumber);
                entity.HasIndex(e => e.TwoFactorEnabled);
                entity.HasIndex(e => e.LockoutEnd);
                entity.HasIndex(e => e.LockoutEnabled);
                entity.HasIndex(e => e.TwoFactorRequired);

                entity.HasOne(a => a.ApplicationUserCategory)
               .WithMany()
               .HasForeignKey(a => a.UserCategoryId)
               .IsRequired();
                entity.Property(e => e.TwoFactorRequired)
                .IsRequired()
                .HasDefaultValue(true);
                entity.Property(e => e.Id)
                .HasColumnType("nvarchar(450)");
                entity.Property(e => e.Name)
                .HasColumnType("nvarchar(100)");
                entity.Property(e => e.LastName)
                .HasColumnType("nvarchar(150)");
                entity.Property(e => e.Occupation)
                .HasColumnType("nvarchar(100)");
                entity.Property(e => e.UserName)
                .HasColumnType("nvarchar(249)");
                entity.Property(e => e.Email)
                .HasColumnType("nvarchar(249)");
                entity.Property(e => e.NormalizedUserName)
                .HasColumnType("nvarchar(249)");
                entity.Property(e => e.PhoneNumber)
                .HasColumnType("nvarchar(100)");
                entity.Property(e => e.OpaqueToken)
                .HasColumnType("nvarchar(64)");
            });

            // APPLICATION USERS ACTIVE HISTORY
            modelBuilder.Entity<ApplicationUserActiveHistory>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.UserId, e.IsActive, e.ActionDate });

                // Indexes on foreign keys
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.UserIdActionedBy);

                // Indexes for columns
                entity.HasIndex(e => e.ActionDate);

                entity.HasKey(e => new { e.HistoryId });
                entity.HasOne(e => e.ApplicationUser)
               .WithMany()
               .HasForeignKey(e => e.UserId)
               .IsRequired();
                entity.HasOne(e => e.UserActionedBy)
               .WithMany()
               .HasForeignKey(e => e.UserIdActionedBy)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);
            });

            // APPLICATION USER CATEGORIES
            modelBuilder.Entity<ApplicationUserCategory>(entity =>
            {
                entity.HasIndex(e => e.UserCategoryId);
            });

            // APPLICATION USER CLAIM
            modelBuilder.Entity<ApplicationUserClaim>(entity =>
            {
                entity.HasDiscriminator<string>("Discriminator")
                .HasValue<ApplicationUserClaim>("ApplicationUser");
            });

            // IDENTITY ROLE
            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable("Roles")
                .HasKey(r => r.Id);
            });

            // IDENTITY USER LOGIN
            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.HasKey(login => new { login.LoginProvider, login.ProviderKey });
            });

            // IDENTITY USER ROLE
            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
            });

            // IDENTITY USER TOKEN
            modelBuilder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name });
            });

            // IMAGE BLOBS
            modelBuilder.Entity<ImageBlob>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.EntityId, e.EntityType, e.CreationDate, e.BlobId });
                entity.HasIndex(e => new { e.ContainerName, e.BlobName });

                entity.HasIndex(e => e.EntityId);
                entity.HasIndex(e => e.ContainerName);
                entity.HasIndex(e => e.BlobName);
                entity.HasIndex(e => e.CreationDate);

                entity.Property(e => e.BlobUrl)
                .HasColumnType("varchar(MAX)");
                entity.Property(e => e.EntityType)
                .HasColumnType("varchar(30)");
                entity.Property(e => e.ContainerName)
                .HasColumnType("varchar(255)");
                entity.Property(e => e.BlobName)
                .HasColumnType("nvarchar(255)");
                entity.Property(e => e.EntityId)
                .HasColumnType("nvarchar(450)");
            });

            // CONSULTANT ROLES QUALITY LEVELS
            modelBuilder.Entity<ConsultantRolesQualityLevels>(entity =>
            {
                entity.HasKey(rq => new { rq.ConsultantRoleId, rq.ConsultantQualityLevelId, rq.ConsultantSeniorityId });
                entity.HasOne(rq => rq.ConsultantSeniority)
                .WithMany()
                .HasForeignKey(rq => rq.ConsultantSeniorityId)
                .IsRequired();
            });

            // CONSULTANTS BENEFITS
            modelBuilder.Entity<ConsultantBenefit>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.BenefitId, e.Name });

                // Índices en fechas
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Amount);
                entity.HasIndex(e => e.BenefitPeriod);
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.EndDate);

                entity.HasKey(c => new { c.BenefitId });
            });

            // CONSULTANTS AND BENEFITS
            modelBuilder.Entity<ConsultantAndBenefit>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.ConsultantId, e.BenefitId });

                // Indexes on foreign keys
                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.BenefitId);

                // Indexes for columns
                entity.HasIndex(e => e.BalanceAmount);

                entity.HasKey(c => new { c.Id });
                entity.HasOne(cc => cc.ConsultantDetail)
                .WithMany()
                .HasForeignKey(cc => cc.ConsultantId)
                .IsRequired();
                entity.HasOne(cc => cc.ConsultantBenefit)
                .WithMany()
                .HasForeignKey(cc => cc.BenefitId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            });


            // CONSULTANTS AND BENEFIT RESET
            modelBuilder.Entity<ConsultantBenefitReset>(entity =>
            {
                // index
                entity.HasIndex(e => new { e.UserIdCreatedBy });


                entity.HasKey(c => new { c.Id });
                entity.HasOne(cc => cc.ApplicationUserCreatedBy)
                .WithMany()
                .HasForeignKey(cc => cc.UserIdCreatedBy)
                .IsRequired();
                entity.Property(e => e.ArrayResetValues)
                .HasColumnType("nvarchar(max)");
            });

            // CONSULTANTS AND BENEFITS HISTORY
            modelBuilder.Entity<ConsultantAndBenefitHistory>(entity =>
            {
                // Indexes on foreign keys
                entity.HasIndex(e => e.ConsultantAndBenefitId);
                entity.HasIndex(e => e.UserCreatedById);
                entity.HasIndex(e => e.ReimbursedBenefitId);

                // Indexes for columns
                entity.HasIndex(e => e.HistoryId);

                entity.HasKey(c => new { c.HistoryId });
                entity.HasOne(cc => cc.ApplicationUser)
                .WithMany()
                .HasForeignKey(cc => cc.UserCreatedById)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(cc => cc.ConsultantAndBenefit)
                .WithMany()
                .HasForeignKey(cc => cc.ConsultantAndBenefitId)
                .IsRequired();
                entity.HasOne(cc => cc.ConsultantReimbursedBenefit)
                .WithMany()
                .HasForeignKey(cc => cc.ReimbursedBenefitId);
            });

            // CONSULTANTS BENEFITS COMPANIES
            modelBuilder.Entity<ConsultantBenefitCompany>(entity =>
            {
                // Indexes on foreign keys
                entity.HasIndex(e => e.CostCenterId);
                entity.HasIndex(e => e.AccountingAccountId);
                entity.HasIndex(e => e.BenefitId);

                // Indexes for columns
                entity.HasIndex(e => e.CompanyId);

                entity.HasKey(c => new { c.ConsultantaBenefitCompanyId });
                entity.HasOne(cc => cc.ConsultantBenefit)
                .WithMany()
                .HasForeignKey(cc => cc.BenefitId)
                .IsRequired();
                entity.HasOne(cc => cc.CostCenter)
                .WithMany()
                .HasForeignKey(cc => cc.CostCenterId)
                .IsRequired();
                entity.HasOne(cc => cc.AccountingAccount)
                .WithMany()
                .HasForeignKey(cc => cc.AccountingAccountId)
                .IsRequired();
            });

            // CONSULTANTS BENEFITS CATEGORIES
            modelBuilder.Entity<ConsultantBenefitCategory>(entity =>
            {
                // Indexes on foreign keys
                entity.HasIndex(e => e.BenefitId);

                // Indexes for columns
                entity.HasIndex(e => e.Name);

                entity.HasKey(c => new { c.BenefitCategoryId });
                entity.HasOne(cb => cb.ConsultantBenefit)
                .WithMany()
                .HasForeignKey(cb => cb.BenefitId)
                .IsRequired();
            });

            // CONSULTANTS REIMBURSED BENEFITS
            modelBuilder.Entity<ConsultantReimbursedBenefit>(entity =>
            {
                entity.HasIndex(e => new { e.ConsultantId, e.TransactionStatusId, e.BenefitId });
                entity.HasIndex(e => new { e.ConsultantId, e.DateToBeReimbursed })
      .IncludeProperties(e => new { e.TransactionStatusId, e.BenefitId, e.BenefitCategoryId, e.AmountReimbursed });


                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.BenefitId);
                entity.HasIndex(e => e.AmountReimbursed);
                entity.HasIndex(e => e.DateToBeReimbursed);
                entity.HasIndex(e => e.ConsultantIdCreatedBy);
                entity.HasIndex(e => e.ConsultantIdLastUpdatedBy);
                entity.HasIndex(e => e.BenefitCategoryId);
                entity.HasIndex(e => e.TransactionStatusId);

                entity.HasKey(c => new { c.ReimbursedBenefitId });
                entity.HasOne(cb => cb.ConsultantBenefit)
                .WithMany()
                .HasForeignKey(cb => cb.BenefitId)
                .IsRequired();
                entity.HasOne(cb => cb.ConsultantDetailBenefit)
                .WithMany()
                .HasForeignKey(cb => cb.ConsultantId)
                .IsRequired();
                entity.HasOne(cb => cb.ConsultantDetailCreatedBy)
                .WithMany()
                .HasForeignKey(cb => cb.ConsultantIdCreatedBy)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(cb => cb.ConsultantDetailUpdatedBy)
                .WithMany()
                .HasForeignKey(cb => cb.ConsultantIdLastUpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(cc => cc.ConsultantBenefitCategory)
                .WithMany()
                .HasForeignKey(cc => cc.BenefitCategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(cc => cc.TransactionStatus)
                .WithMany()
                .HasForeignKey(cc => cc.TransactionStatusId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                entity.Property(d => d.DateToBeReimbursed)
                .HasColumnType("date")
                .IsRequired();
            });

            // CONSULTANT PAYMENTS DEBITS AND CREDITS
            modelBuilder.Entity<ConsultantPaymentDebitsCredits>(entity =>
            {
                entity.HasIndex(e => new { e.ConsultantId, e.TransactionStatusId, e.TransactionTypeId, e.ActionDateWithinFortnight });
                entity.HasIndex(e => new { e.CreationDate, e.LastUpdateDate });

                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.AccountingAccountId);
                entity.HasIndex(e => e.CostCenterId);
                entity.HasIndex(e => e.TransactionStatusId);
                entity.HasIndex(e => e.TransactionTypeId);
                entity.HasIndex(e => e.ConsultantIdCreatedBy);
                entity.HasIndex(e => e.ConsultantIdLastUpdatedBy);
                entity.HasIndex(e => e.Quantity);
                entity.HasIndex(e => e.Amount);

                entity.HasKey(c => new { c.ConsultantPaymentDebitsCreditsId });
                entity.HasOne(cp => cp.ConsultantDetail)
                .WithMany()
                .HasForeignKey(cp => cp.ConsultantId)
                .IsRequired();
                entity.HasOne(cp => cp.AccountingAccount)
                .WithMany()
                .HasForeignKey(cp => cp.AccountingAccountId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(cp => cp.CostCenter)
                .WithMany()
                .HasForeignKey(cp => cp.CostCenterId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(cp => cp.TransactionStatus)
                .WithMany()
                .HasForeignKey(cp => cp.TransactionStatusId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(cp => cp.TransactionType)
                .WithMany()
                .HasForeignKey(cp => cp.TransactionTypeId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(cp => cp.ConsultantDetailCreatedBy)
                .WithMany()
                .HasForeignKey(cp => cp.ConsultantIdCreatedBy)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(cp => cp.ConsultantDetailUpdatedBy)
                .WithMany()
                .HasForeignKey(cp => cp.ConsultantIdLastUpdatedBy);
                entity.Property(d => d.ActionDateWithinFortnight)
                .HasColumnType("date")
                .IsRequired();
            });

            // BANK ACCOUNTS
            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.HasKey(c => new { c.BankAccountId });
                entity.Property(c => c.BankAccountName)
                .HasColumnType("varchar(40)");
                entity.Property(c => c.BankAccountCode)
               .HasColumnType("varchar(20)");
                entity.Property(c => c.IsActive)
               .HasColumnType("varchar(1)");
                entity.Property(c => c.CompanyId)
               .HasColumnType("varchar(8)");
            });

            // COMPANIES
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.Name);

                entity.HasKey(c => new { c.CompanyId });
                entity.Property(c => c.CompanyId)
                .HasColumnType("varchar(8)");
            });

            // COST CENTER
            modelBuilder.Entity<CostCenter>(entity =>
            {
                entity.HasIndex(e => new { e.CostCenterId, e.Description });

                entity.HasIndex(e => e.CostCenterId);
                entity.HasIndex(e => e.CostCenterCode);
            });

            // COST CENTER ACCOUNTING ACCOUNT
            modelBuilder.Entity<CostCenterAccountingAccount>(entity =>
            {
                entity.HasIndex(e => e.CostCenterId);
                entity.HasIndex(e => e.AccountingAccountId);
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.Status);

                entity.HasKey(ca => new { ca.CostCenterAccountingAccountId });
                entity.HasOne(cc => cc.CostCenter)
                .WithMany()
                .HasForeignKey(cc => cc.CostCenterId)
                .IsRequired();
                entity.HasOne(cc => cc.AccountingAccount)
                .WithMany()
                .HasForeignKey(cc => cc.AccountingAccountId)
                .IsRequired();
            });

            // CONSULTANT DETAILS 
            modelBuilder.Entity<ConsultantDetail>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.UserId, e.ConsultantId });
                entity.HasIndex(e => new { e.ConsultantId, e.ConsultantHolidayId });
                entity.HasIndex(e => e.ConsultantId)
        .HasDatabaseName("IX_CD_ConsultantId")
        .IncludeProperties(e => new { e.UserId });

                // Indexes on foreign keys
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IdCountry);
                entity.HasIndex(e => e.UserCreatedBy);
                entity.HasIndex(e => e.UserLastUpdatedBy);
                entity.HasIndex(e => e.PaymentMethodId);
                entity.HasIndex(e => e.ConsultantHolidayId);

                // Indexes for columns
                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.Phone2);
                entity.HasIndex(e => e.Address);
                entity.HasIndex(e => e.PersonalEmail);
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.PaymentPeriod);
                entity.HasIndex(e => e.WorkingModel);
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.OldConsultantSystemStartDate);

                entity.HasKey(rq => new { rq.ConsultantId });
                entity.HasOne(cc => cc.ApplicationUser)
                .WithMany()
                .HasForeignKey(CC => CC.UserId)
                .IsRequired();
                entity.HasOne(cc => cc.ApplicationUserCreated)
               .WithMany()
               .HasForeignKey(CC => CC.UserCreatedBy);
                entity.HasOne(cc => cc.ApplicationUserUpdated)
               .WithMany()
               .HasForeignKey(CC => CC.UserLastUpdatedBy);
                entity.HasOne(cc => cc.Country)
                .WithMany()
                .HasForeignKey(CC => CC.IdCountry)
                .IsRequired();
                entity.HasOne(p => p.PaymentMethod)
                .WithMany()
                .HasForeignKey(p => p.PaymentMethodId);
                entity.HasOne(x => x.ConsultantHoliday)
                .WithMany()
                .HasForeignKey(x => x.ConsultantHolidayId);
                entity.Ignore(c => c.ProjectsConsultantsAssigned)
        .Ignore(c => c.ReportingMyTimeMovements);
                entity.Property(d => d.StartDate)
                .HasColumnType("date")
                .IsRequired();
                entity.Property(d => d.OldConsultantSystemStartDate)
                .HasColumnType("date");
            });

            // CONSULTANT HOLIDAY
            modelBuilder.Entity<ConsultantHoliday>(entity =>
            {
                entity.HasIndex(e => e.ConsultantHolidayId);
                entity.HasIndex(e => e.CreatedBy);
                entity.HasIndex(e => e.Name);

                entity.HasOne(cc => cc.ApplicationUser)
                .WithMany()
                .HasForeignKey(CC => CC.CreatedBy)
                .IsRequired();
            });

            // GLOBAL CONSECUTIVES
            modelBuilder.Entity<GlobalConsecutive>(entity =>
            {
                entity.HasIndex(e => e.GlobalConsecutiveId);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.CompanyId);

                entity.Property(c => c.Name)
                 .HasColumnType("varchar(80)");
                entity.Property(c => c.CompanyId)
                 .HasColumnType("varchar(8)");

                entity.HasOne(c => c.Company)
                .WithMany()
                .HasForeignKey(c => c.CompanyId)
                .IsRequired();
            });

            // HOLIDAY DATE
            modelBuilder.Entity<ConsultantHolidayDate>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.ConsultantHolidayId, e.Date });

                entity.HasIndex(e => e.ConsultantHolidayId);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.CreatedBy);
                entity.HasIndex(e => e.UpdatedBy);
                entity.HasIndex(e => e.Name);

                entity.HasOne(cc => cc.ConsultantHoliday)
                .WithMany()
                .HasForeignKey(CC => CC.ConsultantHolidayId)
                .IsRequired();
                entity.HasOne(cc => cc.ApplicationUserCreated)
                .WithMany()
                .HasForeignKey(CC => CC.CreatedBy)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(cc => cc.ApplicationUserUpdated)
                .WithMany()
                .HasForeignKey(CC => CC.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
                entity.Property(d => d.Date)
                .HasColumnType("date")
                .IsRequired();
            });

            // DOCUMENTS CC NOTIFICATIONS
            modelBuilder.Entity<DocumentsCCNotification>(entity =>
            {
                entity.HasKey(d => new { d.DocumentCCId, d.NotificationId });
            });

            // DOCUMENT CC
            modelBuilder.Entity<DocumentCC>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.DocumentType, e.DocumentDate, e.CompanyId, e.ClientId });

                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.DocumentCCSubtypeId);
                entity.HasIndex(e => e.DocumentType);

                entity.HasOne(p => p.DocumentCCSubtype)
              .WithMany()
              .HasForeignKey(p => p.DocumentCCSubtypeId)
              .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Company)
              .WithMany()
              .HasForeignKey(p => p.CompanyId);
                //   entity.HasOne(p => p.DocumentCCType)
                //.WithMany()
                //.HasForeignKey(p => p.DocumentType)
                entity.Property(c => c.CompanyId)
                 .HasColumnType("varchar(8)");
            });

            // DOCUMENT CC SUBTYPE
            modelBuilder.Entity<DocumentCCSubtype>(entity =>
            {
                entity.HasIndex(e => e.AccountingAccountId);
                entity.HasIndex(e => e.CostCenterId);
                entity.HasIndex(e => e.DocumentTypeId);
                entity.HasIndex(e => e.CompanyId);

                entity.HasOne(p => p.CostCenter)
              .WithMany()
              .HasForeignKey(p => p.CostCenterId)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.AccountingAccount)
              .WithMany()
              .HasForeignKey(p => p.AccountingAccountId)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Company)
              .WithMany()
              .HasForeignKey(p => p.CompanyId)
              .IsRequired();
                entity.HasOne(p => p.DocumentType)
             .WithMany()
             .HasForeignKey(p => p.DocumentTypeId)
             .IsRequired();
            });

            // DOCUMENT TYPES
            modelBuilder.Entity<DocumentType>(entity =>
            {
                entity.HasIndex(e => e.TransactionTypeId);

                entity.HasOne(p => p.TransactionType)
             .WithMany()
             .HasForeignKey(p => p.TransactionTypeId)
             .IsRequired();

            });

            // CLIENT
            modelBuilder.Entity<Client>(entity =>
            {
                // Composite index
                entity.HasIndex(e => e.ClientId)
        .HasDatabaseName("IX_CLIENT_KeyColumns")
        .IncludeProperties(e => new { e.CompanyId, e.LimitNumHoursForOverTime, e.OverTimeAmount });

                // Indexes for columns
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Contact);
                entity.HasIndex(e => e.ContactOccupation);
                entity.HasIndex(e => e.PaymentCondition);
                entity.HasIndex(e => e.Discount);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.ClientCategory);
                entity.HasIndex(e => e.ClientClass);
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.LatePaymentFee);
                entity.HasIndex(e => e.AllowSentLatePaymentNotifications);
                entity.HasIndex(e => e.SuccessManager);

                entity.Property(p => p.LatePaymentFee)
                .HasColumnType("decimal(18, 4)");

            });

            // CLIENT ACCOUNTING CATEGORY
            modelBuilder.Entity<ClientAccountingCategory>(entity =>
            {
                // Indexes for columns
                entity.HasIndex(e => e.AccountingAccountIdSalesReturn);
                entity.HasIndex(e => e.AccountingAccountIdSalesDiscounts);
                entity.HasIndex(e => e.CostCenterIdSalesReturn);
                entity.HasIndex(e => e.CostCenterIdSalesDiscounts);
                entity.HasIndex(e => e.CompanyId);

                entity.Property(c => c.Description)
                 .HasColumnType("varchar(50)");
                entity.HasOne(p => p.AccountingAccountSalesReturn)
              .WithMany()
              .HasForeignKey(p => p.AccountingAccountIdSalesReturn)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.AccountingAccountSalesDiscount)
              .WithMany()
              .HasForeignKey(p => p.AccountingAccountIdSalesDiscounts)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Company)
              .WithMany()
              .HasForeignKey(p => p.CompanyId)
              .IsRequired();

            });

            // CONSULTANT PAYMENTS
            modelBuilder.Entity<ConsultantPayment>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.ConsultantId, e.StartDatePeriod, e.EndDatePeriod });

                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.UserCreatedBy);
                entity.HasIndex(e => e.UserLastUpdatedBy);
                entity.HasIndex(e => e.PaymentMethodId);
                entity.HasIndex(e => e.BankAccountId);
                entity.HasIndex(e => e.AccountingDate);
                entity.HasIndex(e => e.AccountPayableId);
                entity.HasIndex(e => e.Voided);
                entity.HasIndex(e => new { e.ReferenceNumber, e.BankAccountId }).IsUnique();

                entity.HasKey(c => new { c.ConsultantPaymentId });
                entity.HasOne(cp => cp.ConsultantDetail)
                .WithMany()
                .HasForeignKey(cp => cp.ConsultantId)
                .IsRequired();
                entity.HasOne(p => p.PaymentMethod)
               .WithMany()
               .HasForeignKey(p => p.PaymentMethodId)
               .IsRequired();
                entity.HasOne(p => p.ApplicationUserCreatedBy)
              .WithMany()
              .HasForeignKey(p => p.UserCreatedBy)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.ApplicationUserUpdatedBy)
              .WithMany()
              .HasForeignKey(p => p.UserLastUpdatedBy)
              .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.BankAccount)
               .WithMany()
               .HasForeignKey(p => p.BankAccountId)
               .IsRequired();
                entity.Property(d => d.AccountingDate)
                 .HasColumnType("date")
                 .IsRequired();
                entity.Property(d => d.StartDatePeriod)
                 .HasColumnType("date")
                 .IsRequired();
                entity.Property(d => d.EndDatePeriod)
                 .HasColumnType("date")
                 .IsRequired();
                entity.Property(c => c.CompanyId)
                 .HasColumnType("varchar(8)");
                entity.HasOne(p => p.AccountPayable)
              .WithMany()
              .HasForeignKey(p => p.AccountPayableId)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
                entity.Property(r => r.ReferenceNumber)
                .HasColumnType("varchar(85)")
                .IsRequired();
            });

            // CONSULTANT AND POSITIONS
            modelBuilder.Entity<ConsultantAndPosition>(entity =>
            {
                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.ConsultantPositionId);

                entity.HasKey(cp => new { cp.ConsultantId, cp.ConsultantPositionId });
            });

            // INTERVIEWS
            modelBuilder.Entity<Interview>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.ConsultantId, e.Date, e.TransactionStatusId });

                entity.HasIndex(e => e.InterviewId);
                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.ConsultantIdCreatedBy);
                entity.HasIndex(e => e.ConsultantIdLastUpdatedBy);
                entity.HasIndex(e => e.TransactionStatusId);
                entity.HasIndex(e => e.DurationMinutes);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.Detail);

                entity.HasKey(c => new { c.InterviewId });
                entity.HasOne(cp => cp.ConsultantDetail)
                .WithMany()
                .HasForeignKey(cp => cp.ConsultantId)
                .IsRequired();
                entity.HasOne(cp => cp.ConsultantDetailCreatedBy)
                .WithMany()
                .HasForeignKey(cp => cp.ConsultantIdCreatedBy)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(cp => cp.ConsultantDetailUpdatedBy)
                .WithMany()
                .HasForeignKey(cp => cp.ConsultantIdLastUpdatedBy);
                entity.HasOne(cc => cc.TransactionStatus)
                .WithMany()
                .HasForeignKey(cc => cc.TransactionStatusId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                entity.Property(d => d.Date)
                .HasColumnType("date")
                .IsRequired();
            });

            // JOURNAL
            modelBuilder.Entity<JournalAccountPayable>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.CompanyId, e.TransactionStatusId });

                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.Entry);
                entity.HasIndex(e => e.AccountingPackage);
                entity.HasIndex(e => e.UserCreatedBy);
                entity.HasIndex(e => e.UserLastUpdatedBy);
                entity.HasIndex(e => e.AccountingDate);
                entity.HasIndex(e => e.TransactionStatusId);
                entity.HasIndex(e => e.StartDatePeriod);
                entity.HasIndex(e => e.EndDatePeriod);

                entity.HasKey(c => new { c.JournalId });
                entity.Property(c => c.Entry)
                 .HasColumnType("varchar(10)");
                entity.Property(c => c.AccountingPackage)
                 .HasColumnType("varchar(4)");
                entity.Property(c => c.EntryType)
                 .HasColumnType("varchar(4)");
                entity.HasOne(p => p.ApplicationUserCreatedBy)
              .WithMany()
              .HasForeignKey(p => p.UserCreatedBy)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.ApplicationUserUpdatedBy)
              .WithMany()
              .HasForeignKey(p => p.UserLastUpdatedBy)
              .OnDelete(DeleteBehavior.Restrict);
                entity.Property(d => d.AccountingDate)
                 .HasColumnType("date")
                 .IsRequired();
                entity.Property(c => c.CompanyId)
                 .HasColumnType("varchar(8)");
                entity.HasOne(p => p.Company)
               .WithMany()
               .HasForeignKey(p => p.CompanyId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.TransactionStatus)
              .WithMany()
              .HasForeignKey(p => p.TransactionStatusId)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
                entity.Property(d => d.StartDatePeriod)
                 .HasColumnType("date")
                 .IsRequired();
                entity.Property(d => d.EndDatePeriod)
                 .HasColumnType("date")
                 .IsRequired();
            });

            // JOURNAL ENTRIES
            modelBuilder.Entity<JournalAccountPayableEntry>(entity =>
            {
                entity.HasIndex(e => e.CostCenterId);
                entity.HasIndex(e => e.AccountingAccountId);
                entity.HasIndex(e => e.Debit);
                entity.HasIndex(e => e.Credit);
                entity.HasIndex(e => e.AccountPayableId);

                entity.HasKey(c => new { c.JournalEntryId });
                entity.HasOne(p => p.CostCenter)
              .WithMany()
              .HasForeignKey(p => p.CostCenterId)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.AccountPayable)
              .WithMany()
              .HasForeignKey(p => p.AccountPayableId)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.AccountingAccount)
              .WithMany()
              .HasForeignKey(p => p.AccountingAccountId)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.JournalAccountPayable)
              .WithMany()
              .HasForeignKey(p => p.JournalId)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
                entity.Property(c => c.Reference)
                 .HasColumnType("varchar(249)");
                entity.Property(e => e.Debit)
                .HasColumnType("decimal(18, 6)");
                entity.Property(e => e.Credit)
                .HasColumnType("decimal(18, 6)");
            });

            // LEDGER MOVEMENT
            modelBuilder.Entity<LedgerMovement>(entity =>
            {
                entity.HasIndex(e => e.AccountingAccountId);
                entity.HasIndex(e => e.CostCenterId);
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.LocalDebit);
                entity.HasIndex(e => e.LocalCredit);
            });

            // PARTNERS
            modelBuilder.Entity<Partner>(entity =>
            {
                entity.HasKey(x => new { x.PartnerId });
                entity.HasOne(c => c.Country)
                .WithMany()
                .HasForeignKey(c => c.IdCountry)
                .IsRequired();
            });

            // PRODUCTS
            modelBuilder.Entity<Product>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.Name, e.Alias, e.ProductCode });
                entity.HasIndex(e => e.ProductCode)
        .HasDatabaseName("IX_PRODUCTS_Code")
        .IncludeProperties(e => e.ProductId);

                entity.HasIndex(e => e.ProductId);

                entity.HasKey(p => new { p.ProductId });
                entity.Property(c => c.ProductCode)
                  .HasColumnType("varchar(10)");
                entity.Property(c => c.Name)
                  .HasColumnType("varchar(150)");
                entity.Property(c => c.Alias)
                  .HasColumnType("varchar(150)");
                entity.Property(c => c.Detail)
                  .HasColumnType("varchar(300)");
            });

            // PRODUCTS CLIENTS COMPANIES ACCOUNTING CONFIG
            modelBuilder.Entity<ProductClientCompanyAccountingConfigForBilling>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.ProductId, e.ClientId, e.CompanyId })
    .HasDatabaseName("IX_ACC_ProductClientCompany")
    .IncludeProperties(e => new { e.TaxPercentage });
                entity.HasIndex(e => new { e.ClientId, e.CompanyId, e.MovementTypeId })
        .HasDatabaseName("IX_PCC_ConfigMatch")
        .IncludeProperties(e => new { e.ProductId, e.TaxPercentage });

                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.MovementTypeId);
                entity.HasIndex(e => e.CostCenterIdSales);
                entity.HasIndex(e => e.CostCenterIdSalesReturn);
                entity.HasIndex(e => e.CostCenterIdSalesDiscount);
                entity.HasIndex(e => e.CostCenterIdTaxPercentage);
                entity.HasIndex(e => e.AccountingAccountIdSales);
                entity.HasIndex(e => e.AccountingAccountIdSalesReturn);
                entity.HasIndex(e => e.AccountingAccountIdSalesDiscount);
                entity.HasIndex(e => e.AccountingAccountIdTaxPercentage);

                entity.HasKey(pcca => new { pcca.ProductId, pcca.ClientId, pcca.CompanyId });

                entity.HasOne(p => p.Product)
                .WithMany()
                .HasForeignKey(p => p.ProductId)
                .IsRequired();
                entity.HasOne(p => p.Client)
                .WithMany()
                .HasForeignKey(p => p.ClientId)
                .IsRequired();
                entity.HasOne(p => p.Company)
                .WithMany()
                .HasForeignKey(p => p.CompanyId)
                .IsRequired();
                entity.HasOne(p => p.ReportingMyTimeMovementType)
                .WithMany()
                .HasForeignKey(p => p.MovementTypeId);
                entity.HasOne(p => p.CostCenterSales)
                .WithMany()
                .HasForeignKey(p => p.CostCenterIdSales)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
                entity.HasOne(p => p.CostCenterSalesReturn)
                .WithMany()
                .HasForeignKey(p => p.CostCenterIdSalesReturn)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
                entity.HasOne(p => p.CostCenterSalesDiscount)
                .WithMany()
                .HasForeignKey(p => p.CostCenterIdSalesDiscount)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
                entity.HasOne(p => p.CostCenterTaxPercentage)
                .WithMany()
                .HasForeignKey(p => p.CostCenterIdTaxPercentage);
                entity.HasOne(p => p.AccountingAccountSales)
                .WithMany()
                .HasForeignKey(p => p.AccountingAccountIdSales)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
                entity.HasOne(p => p.AccountingAccountSalesReturn)
                .WithMany()
                .HasForeignKey(p => p.AccountingAccountIdSalesReturn)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
                entity.HasOne(p => p.AccountingAccountSalesDiscount)
                .WithMany()
                .HasForeignKey(p => p.AccountingAccountIdSalesDiscount)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
                entity.HasOne(p => p.AccountingAccountTaxPercentage)
                .WithMany()
                .HasForeignKey(p => p.AccountingAccountIdTaxPercentage);
            });

            // PROJECTS
            modelBuilder.Entity<Project>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.IsActive, e.ClientId, e.SuccessManagerId, e.StartDate, e.Name });
                entity.HasIndex(e => new { e.ProjectId, e.ClientId, e.SuccessManagerId});

                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.CreatedBy);
                entity.HasIndex(e => e.UpdatedBy);
                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.SuccessManagerId);
                entity.HasIndex(e => e.ClientHasTrackingTool);
                entity.HasIndex(e => e.IsBillable);
                entity.HasIndex(e => e.Name);

                entity.HasKey(p => new { p.ProjectId });
                entity.HasOne(p => p.ApplicationUserCreated)
                .WithMany()
                .HasForeignKey(p => p.CreatedBy)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.ApplicationUserUpdated)
                .WithMany()
                .HasForeignKey(p => p.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Client)
                .WithMany()
                .HasForeignKey(p => p.ClientId)
                .IsRequired();
                entity.HasOne(p => p.ConsultantDetail)
                .WithMany()
                .HasForeignKey(p => p.SuccessManagerId)
                .IsRequired();
            });

            // PROJECTS CONSULTANTS ASSIGNED
            modelBuilder.Entity<ProjectConsultantAssigned>(entity =>
            {
                // Indexes
                entity.HasIndex(e => new { e.ConsultantId, e.ProjectId });
                entity.HasIndex(e => new { e.ConsultantId, e.ProjectConsultantAssignedId });
                entity.HasIndex(e => new { e.ConsultantId, e.ProjectId, e.ProjectConsultantAssignedId });
                entity.HasIndex(e => e.ProjectConsultantAssignedId);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.ConsultantId);

                //Columns
                entity.HasKey(p => new { p.ProjectConsultantAssignedId });
                entity.HasOne(p => p.Project)
                .WithMany()
                .HasForeignKey(p => p.ProjectId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(cd => cd.ConsultantDetail)
                .WithMany()
                .HasForeignKey(cd => cd.ConsultantId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            });

            // PROJECTS CONSULTANTS ASSIGNED HISTORY
            modelBuilder.Entity<ProjectConsultantAssignedHistory>(entity =>
            {
                // Indexes
                entity.HasIndex(p => new { p.ProjectConsultantAssignedId, p.IsActive, p.MonthlySalary, p.HourlySalary })
                .HasFilter("[IsActive] = 1");
                entity.HasIndex(e => new { e.IsActive, e.AccessToTrackingTool });
                entity.HasIndex(e => new { e.ProjectConsultantAssignedId, e.ActionDate, e.Id })
                .HasDatabaseName("IX_ProjectConsultantsAssignedHistory_ProjectConsultantAssignedId_ActionDate_Id")
                .IsDescending(false, true, true); // Define ActionDate and Id as descending
                entity.HasIndex(e => e.ProjectConsultantAssignedId);
                entity.HasIndex(e => e.UserIdActionedBy);
                entity.HasIndex(e => e.ActionDate);
                entity.HasIndex(e => e.PositionId);
                entity.HasIndex(e => e.PartnerId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.ParticipatesInOnCalls);
                entity.HasIndex(e => e.NumHoursForHoliday);

                //Columns
                entity.HasKey(p => new { p.Id });
                entity.HasOne(p => p.ProjectConsultantAssigned)
                .WithMany()
                .HasForeignKey(p => p.ProjectConsultantAssignedId)
                .IsRequired();
                entity.HasOne(a => a.UserActionedBy)
                .WithMany()
                .HasForeignKey(a => a.UserIdActionedBy)
                .IsRequired();
                entity.Property(d => d.ActionDate)
                .HasColumnType("date")
                .IsRequired();
                entity.HasOne(p => p.ConsultantPosition)
                .WithMany()
                .HasForeignKey(p => p.PositionId)
                .IsRequired();
                entity.HasOne(p => p.Partner)
               .WithMany()
               .HasForeignKey(p => p.PartnerId);
                entity.Property(p => p.NumHoursForHoliday)
                .HasDefaultValue(8);
                entity.Property(p => p.MonthlyClientRateNumDays)
                .HasDefaultValue(0m);
                entity.Property(c => c.PrimaryReportTrackingToolName)
                .HasColumnType("varchar(30)");
                entity.Property(c => c.SecondReportTrackingToolName)
                .HasColumnType("varchar(30)");
            });

            // PROJECTS CONSULTANTS PENDING SUBMISSIONS
            modelBuilder.Entity<ProjectConsultantPendingSubmission>(entity =>
            {
                entity.HasIndex(e => new { e.ProjectId, e.ConsultantId, e.StartDate, e.EndDate })
                .IsUnique();
                entity.HasIndex(e => new { e.ConsultantId, e.EndDate });
                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.EndDate);

                entity.HasKey(p => new { p.Id });
                entity.HasOne(p => p.ConsultantDetail)
                .WithMany()
                .HasForeignKey(p => p.ConsultantId)
                .IsRequired();
                entity.HasOne(p => p.Project)
                .WithMany()
                .HasForeignKey(p => p.ProjectId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                entity.Property(d => d.StartDate)
                .HasColumnType("date")
                .IsRequired();
                entity.Property(d => d.EndDate)
                .HasColumnType("date")
                .IsRequired();
            });

            // PROJECTS CONSULTANTS PENDING SUBMISSIONS SENT TIME
            modelBuilder.Entity<ProjectConsultantPendingSubmissionSentTimes>(entity =>
            {
                entity.HasIndex(e => new { e.StartDate, e.EndDate });
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.EndDate);

                entity.HasKey(p => new { p.Id });
                entity.Property(d => d.StartDate)
                .HasColumnType("date")
                .IsRequired();
                entity.Property(d => d.EndDate)
                .HasColumnType("date")
                .IsRequired();
            });

            // PROJECTS USERS SELECTED
            modelBuilder.Entity<ProjectUserSelected>(entity =>
            {
                // Indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ProjectId);
                entity.HasKey(pu => new { pu.ProjectId, pu.UserId });
                entity.HasOne(p => p.ApplicationUser)
               .WithMany()
               .HasForeignKey(p => p.UserId)
               .IsRequired().OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Project)
               .WithMany()
               .HasForeignKey(p => p.ProjectId)
               .IsRequired().OnDelete(DeleteBehavior.Restrict);
            });

            // PAYMENT BOOK ENTRIES PARENT
            modelBuilder.Entity<PaymentBookEntryParent>(entity =>
            {
                entity.HasIndex(e => new { e.CompanyId, e.TransactionStatusId, e.ParentId });
                entity.HasIndex(e => e.ParentId);
                entity.HasIndex(e => e.UserCreatedBy);
                entity.HasIndex(e => e.TransactionStatusId);
                entity.HasIndex(e => e.CompanyId);

                entity.HasOne(p => p.ApplicationUserCreatedBy)
              .WithMany()
              .HasForeignKey(p => p.UserCreatedBy)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
                entity.Property(c => c.CompanyId)
                 .HasColumnType("varchar(8)");
                entity.HasOne(p => p.Company)
               .WithMany()
               .HasForeignKey(p => p.CompanyId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.TransactionStatus)
              .WithMany()
              .HasForeignKey(p => p.TransactionStatusId)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
            });

            // PAYMENT BOOK ENTRIES CHILD
            modelBuilder.Entity<PaymentBookEntryChild>(entity =>
            {
                entity.HasIndex(e => e.ChildId);
                entity.HasIndex(e => e.ParentId);
                entity.HasIndex(e => e.ConsultantPaymentId);
                entity.HasIndex(e => e.Voided);

                entity.HasOne(p => p.PaymentBookEntryParent)
              .WithMany()
              .HasForeignKey(p => p.ParentId)
              .IsRequired();
                entity.HasOne(p => p.ConsultantPayment)
              .WithMany()
              .HasForeignKey(p => p.ConsultantPaymentId)
              .IsRequired();
            });

            // PAYMENT METHOD AND BANK ACCOUNTS
            modelBuilder.Entity<PaymentMethodBankAccount>(entity =>
            {
                entity.HasIndex(e => e.PaymentMethodId);
                entity.HasIndex(e => e.BankAccountId);

                entity.HasKey(cp => new { cp.PaymentMethodId, cp.BankAccountId });

                entity.HasOne(c => c.PaymentMethod)
                .WithMany()
                .HasForeignKey(c => c.PaymentMethodId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.BankAccount)
                .WithMany()
                .HasForeignKey(c => c.BankAccountId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
            });

            // PROJECT CONSULTAN PERIOD DISABLED TRACKING
            modelBuilder.Entity<ProjectConsultantPeriodDisabledTracking>(entity =>
            {
                // Indexes
                entity.HasIndex(e => new { e.ConsultantId, e.ProjectId, e.StartPeriodDate, e.EndPeriodDate });
                entity.HasIndex(e => new { e.ProjectId, e.ConsultantId });
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.StartPeriodDate);
                entity.HasIndex(e => e.EndPeriodDate);
                entity.HasIndex(e => e.CreatedBy);

                //Columns
                entity.HasKey(r => new { r.Id });

                entity.HasOne(p => p.Project)
                .WithMany()
                .HasForeignKey(p => p.ProjectId)
                .IsRequired();

                entity.HasOne(c => c.ConsultantDetail)
                .WithMany()
                .HasForeignKey(c => c.ConsultantId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);

                entity.Property(d => d.StartPeriodDate)
                .HasColumnType("date")
                .IsRequired();

                entity.Property(d => d.EndPeriodDate)
                .HasColumnType("date")
                .IsRequired();

                entity.HasOne(u => u.ApplicationUserCreated)
                .WithMany()
                .HasForeignKey(u => u.CreatedBy)
                .IsRequired().OnDelete(DeleteBehavior.Restrict); ;
            });

            // REPORTING MY TIME COMMENTS
            modelBuilder.Entity<ReportingMyTimeComments>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.SubmissionId);
                entity.HasIndex(e => e.ActionDate);

                entity.HasKey(r => new { r.CommentId });
                entity.HasOne(p => p.Project)
                    .WithMany()
                    .HasForeignKey(p => p.ProjectId)
                    .IsRequired();
                entity.HasOne(c => c.ConsultantDetail)
                .WithMany()
                .HasForeignKey(c => c.ConsultantId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.ReportingMyTimeMovementSubmission)
                .WithMany()
                .HasForeignKey(c => c.SubmissionId)
                .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.ApplicationUser)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
                entity.Property(d => d.ActionDate)
                .HasColumnType("date")
                .IsRequired();
            });

            // CONSULTANT POSITIONS ACCOUNTING CONFIGURATION
            modelBuilder.Entity<ConsultantPositionAccountingConfiguration>(entity =>
            {
                entity.HasIndex(e => e.CostCenterId);
                entity.HasIndex(e => e.AccountingAccountId);
                entity.HasIndex(e => e.MovementTypeId);
                entity.HasIndex(e => e.PositionId);

                entity.HasKey(r => new { r.Id });
                entity.HasOne(c => c.CostCenter)
                .WithMany()
                .HasForeignKey(c => c.CostCenterId)
                .IsRequired();
                entity.HasOne(a => a.AccountingAccount)
                .WithMany()
                .HasForeignKey(a => a.AccountingAccountId)
                .IsRequired();
                entity.HasOne(r => r.ConsultantPosition)
                .WithMany()
                .HasForeignKey(r => r.PositionId)
                .IsRequired();
                entity.HasOne(m => m.MovementType)
               .WithMany()
               .HasForeignKey(m => m.MovementTypeId)
               .IsRequired();
                entity.Property(d => d.CompanyId)
                .HasColumnType("varchar")
                .IsRequired();
            });

            // REPORTING MY TIME MOVEMENTS
            modelBuilder.Entity<ReportingMyTimeMovement>(entity =>
            {
                entity.HasIndex(e => new { e.ProjectId, e.ConsultantId, e.ActionDate });
                entity.HasIndex(e => new { e.ProjectId, e.ConsultantId, e.ActionDate, e.IsBillable, e.TransactionStatusId });
                entity.HasIndex(e => new { e.ProjectId, e.ConsultantId });

                entity.HasIndex(e => e.ActionDate);
                entity.HasIndex(e => e.Quantity);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.TransactionStatusId);
                entity.HasIndex(e => e.MovementTypeId);
                entity.HasIndex(e => e.IsBillable);
                entity.HasIndex(e => e.UserIdLastUpdatedBy);

                entity.HasKey(r => new { r.MovementId });
                entity.HasOne(p => p.Project)
                .WithMany()
                .HasForeignKey(p => p.ProjectId)
                .IsRequired();
                entity.HasOne(c => c.ConsultantDetail)
                .WithMany()
                .HasForeignKey(c => c.ConsultantId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.TransactionStatus)
                .WithMany()
                .HasForeignKey(t => t.TransactionStatusId)
                .IsRequired();
                entity.HasOne(r => r.ReportingMyTimeMovementType)
                .WithMany()
                .HasForeignKey(r => r.MovementTypeId)
                .IsRequired();
                entity.Property(d => d.ActionDate)
                .HasColumnType("date")
                .IsRequired();
                entity.Property(d => d.TimeFrom)
                .HasColumnType("varchar");
                entity.Property(d => d.TimeTo)
                .HasColumnType("varchar");
                entity.Property(i => i.IsBillable)
                .HasDefaultValue(true)
                .IsRequired();
                entity.Property(c => c.NonBillableReason)
                  .HasColumnType("varchar(800)");
                entity.Property(c => c.UserIdLastUpdatedBy)
                  .HasColumnType("nvarchar(450)");
                entity.HasOne(t => t.UserUpdatedBy)
               .WithMany()
               .HasForeignKey(t => t.UserIdLastUpdatedBy);
            });

            // REPORTING MY TIME MOVEMENT BLOBS
            modelBuilder.Entity<ReportingMyTimeMovementBlob>(entity =>
            {
                entity.HasIndex(e => e.MovementId);

                entity.HasKey(r => new { r.InternalBlobId });
                entity.HasOne(p => p.ReportingMyTimeMovement)
                .WithMany()
                .HasForeignKey(p => p.MovementId)
                .IsRequired();
                entity.Property(c => c.ContainerId)
                .HasColumnType("VARCHAR(255)");
                entity.Property(c => c.ContentType)
                .HasColumnType("VARCHAR(255)");
                entity.Property(c => c.PrimaryReportTrackingToolName)
                .HasColumnType("varchar(30)");
                entity.Property(c => c.SecondReportTrackingToolName)
                .HasColumnType("varchar(30)");
            });

            // REPORTING MY TIME MOVEMENT SUBMISSIONS
            modelBuilder.Entity<ReportingMyTimeMovementSubmission>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.ConsultantId, e.ProjectId, e.StartPeriodDate, e.EndPeriodDate });
                entity.HasIndex(e => new { e.TransactionStatusId, e.StartPeriodDate, e.EndPeriodDate });
                entity.HasIndex(e => new { e.ProjectId, e.ConsultantId });

                // Indexes on foreign keys
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.TransactionStatusId);
                entity.HasIndex(e => e.SubmissionDate);
                entity.HasIndex(e => e.LastSubmissionDate);
                entity.HasIndex(e => e.SubmissionId);
                entity.HasIndex(e => e.StartPeriodDate);
                entity.HasIndex(e => e.EndPeriodDate);

                entity.HasKey(r => new { r.SubmissionId });
                entity.HasOne(p => p.Project)
                .WithMany()
                .HasForeignKey(p => p.ProjectId)
                .IsRequired();
                entity.HasOne(c => c.ConsultantDetail)
                .WithMany()
                .HasForeignKey(c => c.ConsultantId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.TransactionStatus)
                .WithMany()
                .HasForeignKey(t => t.TransactionStatusId)
                .IsRequired();
                entity.Property(d => d.StartPeriodDate)
                .HasColumnType("date")
                .IsRequired();
                entity.Property(d => d.EndPeriodDate)
                .HasColumnType("date")
                .IsRequired();
            });

            // REPORTING MY TIME MOVEMENTS TYPES
            modelBuilder.Entity<ReportingMyTimeMovementType>(entity =>
            {
                entity.HasIndex(e => new { e.MovementTypeId, e.IsPayable });

                entity.HasIndex(e => e.MovementTypeId);
                entity.HasIndex(e => e.IsPayable);
                entity.HasIndex(e => e.Name);
            });

            // TRANSACTION STATUSES
            modelBuilder.Entity<TransactionStatus>(entity =>
            {
                entity.HasIndex(e => new { e.TransactionStatusId, e.Name });
                entity.HasIndex(e => e.TransactionStatusId);
                entity.HasIndex(e => e.Name);
            });

            // TRANSACTION TYPES
            modelBuilder.Entity<TransactionType>(entity =>
            {
                entity.HasIndex(e => new { e.TransactionTypeId, e.Name });
                entity.HasIndex(e => e.TransactionTypeId);
            });

            // TIME OFF REQUESTS
            modelBuilder.Entity<TimeOffRequest>(entity =>
            {
                entity.HasKey(e => e.TimeOffRequestId);

                entity.HasIndex(e => new { e.ConsultantId, e.TransactionStatusId, e.TimeOffType });
                entity.HasIndex(e => new { e.ConsultantId, e.StartDate, e.EndDate });
                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.TransactionStatusId);
                entity.HasIndex(e => e.TimeOffType);
                entity.HasIndex(e => e.UserCreatedBy);
                entity.HasIndex(e => e.UserActionedBy);

                entity.HasOne(e => e.ConsultantDetail)
                    .WithMany()
                    .HasForeignKey(e => e.ConsultantId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TransactionStatus)
                    .WithMany()
                    .HasForeignKey(e => e.TransactionStatusId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ApplicationUserCreated)
                    .WithMany()
                    .HasForeignKey(e => e.UserCreatedBy)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ApplicationUserActioned)
                    .WithMany()
                    .HasForeignKey(e => e.UserActionedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.StartDate).HasColumnType("date").IsRequired();
                entity.Property(e => e.EndDate).HasColumnType("date").IsRequired();
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasIndex(e => e.DisplayOrder);
                entity.HasIndex(e => e.TeamLeaderId);

                entity.HasOne(e => e.TeamLeader)
                    .WithMany()
                    .HasForeignKey(e => e.TeamLeaderId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // CHECK-INS (Weekly Pulse snapshot — exactly one per (Team, Week))
            modelBuilder.Entity<CheckIn>(entity =>
            {
                entity.HasIndex(e => new { e.TeamId, e.WeekStart }).IsUnique();
                entity.HasIndex(e => e.WeekStart);

                entity.Property(e => e.WeekStart).HasColumnType("date").IsRequired();

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ISSUES (Weekly Pulse Living entity — single identity across Weeks, ADR 0001)
            modelBuilder.Entity<Issue>(entity =>
            {
                entity.HasIndex(e => e.TeamId);
                entity.HasIndex(e => e.OriginWeekStart);

                entity.Property(e => e.OriginWeekStart).HasColumnType("date").IsRequired();

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ISSUE HISTORY (week-stamped status/comment rows — one row per change)
            modelBuilder.Entity<IssueHistory>(entity =>
            {
                entity.HasIndex(e => new { e.IssueId, e.WeekStart });

                entity.Property(e => e.WeekStart).HasColumnType("date").IsRequired();

                entity.HasOne(e => e.Issue)
                    .WithMany(i => i.History)
                    .HasForeignKey(e => e.IssueId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // KPI DEFINITIONS (Weekly Pulse structural record — one per metric, per Team)
            modelBuilder.Entity<KpiDefinition>(entity =>
            {
                entity.HasIndex(e => e.TeamId);
                entity.HasIndex(e => e.OwnerId);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Owner)
                    .WithMany()
                    .HasForeignKey(e => e.OwnerId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });

        }
        public DbSet<AccountingAccount> ACCOUNTING_ACCOUNT { get; set; }
        public DbSet<AccountPayable> ACCOUNTS_PAYABLE { get; set; }
        public DbSet<AccountPayableMovement> ACCOUNTS_PAYABLE_MOVEMENTS { get; set; }
        public DbSet<LedgerMovement> LEDGER_MOVEMENT { get; set; }
        public DbSet<DataUpdateDate> DATA_UPDATE_DATES { get; set; }
        public DbSet<ApplicationUser> AspNetUsers { get; set; }
        public DbSet<ApplicationUserActiveHistory> UsersActiveHistory { get; set; }
        public DbSet<ApplicationUserCategory> UserCategories { get; set; }
        public DbSet<ApplicationRoleClaim> ApplicationRoleClaims { get; set; }
        public DbSet<ApplicationUserClaim> ApplicationUserClaims { get; set; }
        public DbSet<ApplicationSystemClaim> APPLICATION_SYSTEM_CLAIMS { get; set; }
        public DbSet<BankAccount> BANK_ACCOUNTS { get; set; }
        public DbSet<CalculatorGlobalConfiguration> CALCULATOR_GLOBAL_CONFIGURATIONS { get; set; }
        public DbSet<CalculatorCostCenterIncreaseConfiguration> CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS { get; set; }
        public DbSet<CalculatorSearchHistory> CALCULATOR_SEARCH_HISTORY { get; set; }
        public DbSet<CalculatorAccountingAccountToIgnore> CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE { get; set; }
        public DbSet<Client> CLIENT { get; set; }
        public DbSet<ClientAccountingCategory> CLIENT_ACCOUNT_CATEGORIES { get; set; }
        public DbSet<Company> COMPANIES { get; set; }
        public DbSet<CostCenter> COST_CENTER { get; set; }
        public DbSet<CostCenterAccountingAccount> COSTS_CENTERS_ACCOUNTING_ACCOUNTS { get; set; }
        public DbSet<Country> COUNTRY { get; set; }
        public DbSet<ConsultantRole> CONSULTANT_ROLES { get; set; }
        public DbSet<ConsultantQualityLevel> CONSULTANT_QUALITY_LEVELS { get; set; }
        public DbSet<ConsultantRolesQualityLevels> CONSULTANT_ROLES_QUALITY_LEVELS { get; set; }
        public DbSet<ConsultantPayment> CONSULTANT_PAYMENTS { get; set; }
        public DbSet<ConsultantPosition> CONSULTANT_POSITIONS { get; set; }
        public DbSet<ConsultantAndPosition> CONSULTANTS_AND_POSITIONS { get; set; }
        public DbSet<ConsultantDetail> CONSULTANT_DETAILS { get; set; }
        public DbSet<ConsultantHoliday> CONSULTANT_HOLIDAYS { get; set; }
        public DbSet<ConsultantHolidayDate> CONSULTANT_HOLIDAY_DATES { get; set; }
        public DbSet<ConsultantSeniority> CONSULTANT_SENIORITIS { get; set; }
        public DbSet<ConsultantBenefit> CONSULTANT_BENEFITS { get; set; }
        public DbSet<ConsultantAndBenefit> CONSULTANTS_AND_BENEFITS { get; set; }
        public DbSet<ConsultantBenefitReset> CONSULTANT_BENEFITS_RESETS { get; set; }
        public DbSet<ConsultantAndBenefitHistory> CONSULTANTS_AND_BENEFITS_HISTORY { get; set; }
        public DbSet<ConsultantBenefitCompany> CONSULTANT_BENEFIT_COMPANIES { get; set; }
        public DbSet<ConsultantBenefitCategory> CONSULTANT_BENEFIT_CATEGORIES { get; set; }
        public DbSet<ConsultantPaymentDebitsCredits> CONSULTANT_PAYMENTS_DEBITS_CREDITS { get; set; }
        public DbSet<ConsultantPositionAccountingConfiguration> CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION { get; set; }
        public DbSet<ConsultantReimbursedBenefit> CONSULTANT_REIMBURSED_BENEFITS { get; set; }
        public DbSet<DocumentCC> DOCUMENTS_CC { get; set; }
        public DbSet<DocumentCCSubtype> DOCUMENTS_CC_SUBTYPES { get; set; }
        public DbSet<DocumentType> DOCUMENTS_TYPES { get; set; }
        public DbSet<GlobalConsecutive> GLOBAL_CONSECUTIVES { get; set; }
        public DbSet<ImageBlob> IMAGE_BLOBS { get; set; }
        public DbSet<Interview> INTERVIEWS { get; set; }
        public DbSet<JournalAccountPayable> JOURNAL_ACCOUNTS_PAYABLE { get; set; }
        public DbSet<JournalAccountPayableEntry> JOURNAL_ACCOUNTS_PAYABLE_ENTRIES { get; set; }
        public DbSet<Partner> PARTNERS { get; set; }
        public DbSet<PaymentMethod> PAYMENT_METHODS { get; set; }
        public DbSet<PaymentBookEntryParent> PAYMENT_BOOK_ENTRIES_PARENT { get; set; }
        public DbSet<PaymentBookEntryChild> PAYMENT_BOOK_ENTRIES_CHILD { get; set; }
        public DbSet<PaymentMethodBankAccount> PAYMENT_METHOD_AND_BANK_ACCOUNTS { get; set; }
        public DbSet<Product> PRODUCTS { get; set; }
        public DbSet<ProductClientCompanyAccountingConfigForBilling> PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG { get; set; }
        public DbSet<Project> PROJECTS { get; set; }
        public DbSet<ProjectConsultantAssigned> PROJECTS_CONSULTANTS_ASSIGNED { get; set; }
        public DbSet<ProjectConsultantAssignedHistory> PROJECTS_CONSULTANTS_ASSIGNED_HISTORY { get; set; }
        public DbSet<ProjectConsultantPeriodDisabledTracking> PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS { get; set; }
        public DbSet<ProjectConsultantPendingSubmission> PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS { get; set; }
        public DbSet<ProjectConsultantPendingSubmissionSentTimes> PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_SENT_TIMES { get; set; }
        public DbSet<ProjectUserSelected> PROJECTS_USERS_SELECTED { get; set; }
        public DbSet<ReportingMyTimeComments> REPORTING_MY_TIME_COMMENTS { get; set; }
        public DbSet<ReportingMyTimeMovement> REPORTING_MY_TIME_MOVEMENTS { get; set; }
        public DbSet<ReportingMyTimeMovementBlob> REPORTING_MY_TIME_MOVEMENT_BLOBS { get; set; }
        public DbSet<ReportingMyTimeMovementType> REPORTING_MY_TIME_MOVEMENT_TYPES { get; set; }
        public DbSet<ReportingMyTimeMovementSubmission> REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS { get; set; }
        public DbSet<TransactionType> TRANSACTION_TYPES { get; set; }
        public DbSet<TransactionStatus> TRANSACTION_STATUSES { get; set; }
        public DbSet<NotificationType> NOTIFICATION_TYPES { get; set; }
        public DbSet<Notification> NOTIFICATIONS { get; set; }
        public DbSet<NotificationStatus> NOTIFICATION_STATUS { get; set; }
        public DbSet<NotificationMedia> NOTIFICATION_MEDIA { get; set; }
        public DbSet<NotificationRecipient> NOTIFICATION_RECIPIENTS { get; set; }
        public DbSet<DocumentsCCNotification> DOCUMENTS_CC_NOTIFICATIONS { get; set; }
        public DbSet<SystemArea> SYSTEM_AREAS { get; set; }
        public DbSet<SystemSubArea> SYSTEM_SUB_AREAS { get; set; }
        public DbSet<TimeOffRequest> TIME_OFF_REQUESTS { get; set; }
        public DbSet<Team> TEAMS { get; set; }
        public DbSet<CheckIn> CHECK_INS { get; set; }
        public DbSet<Issue> ISSUES { get; set; }
        public DbSet<IssueHistory> ISSUE_HISTORIES { get; set; }
        public DbSet<KpiDefinition> KPI_DEFINITIONS { get; set; }
    }
}
