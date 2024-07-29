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

            // APPLICATION USER
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.Id, e.Name, e.LastName });

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
            });
               

            // APPLICATION USER CATEGORIES
            modelBuilder.Entity<ApplicationUserCategory>(entity =>
            {
                entity.HasIndex(e => e.UserCategoryId);
            });

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

            // BANK ACCOUNTS
            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.HasIndex(e => e.CostCenterId);
                entity.HasIndex(e => e.AccountingAccountId);
            });

            modelBuilder.Entity<BankAccount>()
                .HasKey(c => new { c.BankAccountId });
            modelBuilder.Entity<BankAccount>()
                .HasOne(c => c.CostCenter)
                .WithMany()
                .HasForeignKey(c => c.CostCenterId)
                .IsRequired();
            modelBuilder.Entity<BankAccount>()
                .HasOne(c => c.AccountingAccount)
                .WithMany()
                .HasForeignKey(c => c.AccountingAccountId)
                .IsRequired();
            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.Property(c => c.BankAccountName)
                .HasColumnType("varchar(40)");
            });
            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.Property(c => c.BankAccountCode)
                .HasColumnType("varchar(20)");
            });
            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.Property(c => c.IsActive)
                .HasColumnType("varchar(1)");
            });
            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.Property(c => c.CompanyId)
                .HasColumnType("varchar(8)");
            });

            // COMPANIES
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);
            });
            modelBuilder.Entity<Company>()
                .HasKey(c => new { c.CompanyId });
            modelBuilder.Entity<Company>(entity =>
            {
                entity.Property(c => c.CompanyId)
                .HasColumnType("varchar(8)");
            });

            //COST CENTER ACCOUNTING ACCOUNT
            modelBuilder.Entity<CostCenterAccountingAccount>(entity =>
            {
                entity.HasIndex(e => e.CostCenterId);
                entity.HasIndex(e => e.AccountingAccountId);
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.Status);
            });
            modelBuilder.Entity<CostCenterAccountingAccount>()
                .HasKey(ca => new { ca.CostCenterAccountingAccountId });
            modelBuilder.Entity<CostCenterAccountingAccount>()
                .HasOne(cc => cc.CostCenter)
                .WithMany()
                .HasForeignKey(cc => cc.CostCenterId)
                .IsRequired();
            modelBuilder.Entity<CostCenterAccountingAccount>()
                .HasOne(cc => cc.AccountingAccount)
                .WithMany()
                .HasForeignKey(cc => cc.AccountingAccountId)
                .IsRequired();

            //CONSULTANT_DETAIL TABLE
            modelBuilder.Entity<ConsultantDetail>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.UserId, e.ConsultantId });

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
                entity.HasIndex(e => e.ParticipatesInOnCalls);

            });
            modelBuilder.Entity<ConsultantDetail>()
                .HasKey(rq => new { rq.ConsultantId });
            modelBuilder.Entity<ConsultantDetail>()
                .HasOne(cc => cc.ApplicationUser)
                .WithMany()
                .HasForeignKey(CC => CC.UserId)
                .IsRequired();
            modelBuilder.Entity<ConsultantDetail>()
               .HasOne(cc => cc.ApplicationUserCreated)
               .WithMany()
               .HasForeignKey(CC => CC.UserCreatedBy);
            modelBuilder.Entity<ConsultantDetail>()
               .HasOne(cc => cc.ApplicationUserUpdated)
               .WithMany()
               .HasForeignKey(CC => CC.UserLastUpdatedBy);
            modelBuilder.Entity<ConsultantDetail>()
                .HasOne(cc => cc.Country)
                .WithMany()
                .HasForeignKey(CC => CC.IdCountry)
                .IsRequired();
            modelBuilder.Entity<ConsultantDetail>()
                .HasOne(p => p.PaymentMethod)
                .WithMany()
                .HasForeignKey(p => p.PaymentMethodId);
            modelBuilder.Entity<ConsultantDetail>()
                .HasOne(x => x.ConsultantHoliday)
                .WithMany()
                .HasForeignKey(x => x.ConsultantHolidayId);
            modelBuilder.Entity<ConsultantDetail>()
        .Ignore(c => c.ProjectsConsultantsAssigned)
        .Ignore(c => c.ReportingMyTimeMovements);

            //  CONSULTANT HOLIDAY
            modelBuilder.Entity<ConsultantHoliday>(entity =>
            {
                entity.HasIndex(e => e.ConsultantHolidayId);
                entity.HasIndex(e => e.CreatedBy);
                entity.HasIndex(e => e.Name);
            });
            modelBuilder.Entity<ConsultantHoliday>()
                .HasOne(cc => cc.ApplicationUser)
                .WithMany()
                .HasForeignKey(CC => CC.CreatedBy)
                .IsRequired();

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
            });
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
            modelBuilder.Entity<ConsultantHolidayDate>()
                .Property(d => d.Date)
                .HasColumnType("date")
                .IsRequired();

            modelBuilder.Entity<DocumentsCCNotification>()
               .HasKey(d => new { d.DocumentCCId, d.NotificationId });

            // DOCUMENT CC
            modelBuilder.Entity<DocumentCC>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.DocumentType, e.DocumentDate, e.CompanyId, e.ClientId });

                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.CompanyId);
            });

            // CLIENT
            modelBuilder.Entity<Client>(entity =>
            {
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

            });
            modelBuilder.Entity<Client>()
                .Property(p => p.LatePaymentFee)
                .HasColumnType("decimal(18, 4)");

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
            });

            modelBuilder.Entity<ConsultantPayment>()
                .HasKey(c => new { c.ConsultantPaymentId });
            modelBuilder.Entity<ConsultantPayment>()
                .HasOne(cp => cp.ConsultantDetail)
                .WithMany()
                .HasForeignKey(cp => cp.ConsultantId)
                .IsRequired();
            modelBuilder.Entity<ConsultantPayment>()
               .HasOne(p => p.PaymentMethod)
               .WithMany()
               .HasForeignKey(p => p.PaymentMethodId)
               .IsRequired();
            modelBuilder.Entity<ConsultantPayment>()
              .HasOne(p => p.ApplicationUserCreatedBy)
              .WithMany()
              .HasForeignKey(p => p.UserCreatedBy)
              .IsRequired()
              .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ConsultantPayment>()
              .HasOne(p => p.ApplicationUserUpdatedBy)
              .WithMany()
              .HasForeignKey(p => p.UserLastUpdatedBy)
              .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ConsultantPayment>()
              .HasOne(p => p.BankAccount)
              .WithMany()
              .HasForeignKey(p => p.BankAccountId)
              .IsRequired();
            modelBuilder.Entity<ConsultantPayment>()
                .Property(d => d.StartDatePeriod)
                .HasColumnType("date")
                .IsRequired();
            modelBuilder.Entity<ConsultantPayment>()
                .Property(d => d.EndDatePeriod)
                .HasColumnType("date")
                .IsRequired();
            modelBuilder.Entity<ConsultantPayment>(entity =>
            {
                entity.Property(c => c.CompanyId)
                .HasColumnType("varchar(8)");
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
                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.ConsultantIdCreatedBy);
                entity.HasIndex(e => e.ConsultantIdLastUpdatedBy);
                entity.HasIndex(e => e.TransactionStatusId);
                entity.HasIndex(e => e.DurationMinutes);
                entity.HasIndex(e => e.Date);
            });
            modelBuilder.Entity<Interview>()
                .HasKey(c => new { c.InterviewId });
            modelBuilder.Entity<Interview>()
                .HasOne(cp => cp.ConsultantDetail)
                .WithMany()
                .HasForeignKey(cp => cp.ConsultantId)
                .IsRequired();
            modelBuilder.Entity<Interview>()
                .HasOne(cp => cp.ConsultantDetailCreatedBy)
                .WithMany()
                .HasForeignKey(cp => cp.ConsultantIdCreatedBy)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Interview>()
                .HasOne(cp => cp.ConsultantDetailUpdatedBy)
                .WithMany()
                .HasForeignKey(cp => cp.ConsultantIdLastUpdatedBy);
            modelBuilder.Entity<Interview>()
                .HasOne(cc => cc.TransactionStatus)
                .WithMany()
                .HasForeignKey(cc => cc.TransactionStatusId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Interview>()
                .Property(d => d.Date)
                .HasColumnType("date")
                .IsRequired();

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
            //PARTNERS
            modelBuilder.Entity<Partner>()
                .HasKey(x => new { x.PartnerId });
            modelBuilder.Entity<Partner>()
                .HasOne(c => c.Country)
                .WithMany()
                .HasForeignKey(c => c.IdCountry)
                .IsRequired();


            //PROJECTS
            modelBuilder.Entity<Project>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.IsActive, e.ClientId, e.SuccessManagerId, e.StartDate, e.Name });

                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.CreatedBy);
                entity.HasIndex(e => e.UpdatedBy);
                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.SuccessManagerId);
                entity.HasIndex(e => e.ClientHasTrackingTool);
                entity.HasIndex(e => e.IsBillable);
                entity.HasIndex(e => e.Name);
            });
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
            modelBuilder.Entity<ProjectConsultantAssigned>(entity =>
            {
                // Indexes
                entity.HasIndex(e => new { e.ConsultantId, e.ProjectId});
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
                entity.HasIndex(e => new { e.ProjectConsultantAssignedId, e.ActionDate, e.Id });
                entity.HasIndex(e => e.ProjectConsultantAssignedId);
                entity.HasIndex(e => e.UserIdActionedBy);
                entity.HasIndex(e => e.ActionDate);
                entity.HasIndex(e => e.PositionId);
                entity.HasIndex(e => e.PartnerId);
                entity.HasIndex(e => e.IsActive);

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


            // CONSULTANT PAYMENTS
            modelBuilder.Entity<ConsultantPayment>(entity =>
            {
                entity.Property(c => c.CompanyId)
                .HasColumnType("varchar(8)");
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
            modelBuilder.Entity<ReportingMyTimeComments>()
                .HasKey(r => new { r.CommentId });
            modelBuilder.Entity<ReportingMyTimeComments>()
                .HasOne(p => p.Project)
                .WithMany()
                .HasForeignKey(p => p.ProjectId)
                .IsRequired();
            modelBuilder.Entity<ReportingMyTimeComments>()
                .HasOne(c => c.ConsultantDetail)
                .WithMany()
                .HasForeignKey(c => c.ConsultantId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ReportingMyTimeComments>()
                .HasOne(c => c.ReportingMyTimeMovementSubmission)
                .WithMany()
                .HasForeignKey(c => c.SubmissionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ReportingMyTimeComments>()
                .HasOne(c => c.ApplicationUser)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ReportingMyTimeComments>()
                .Property(d => d.ActionDate)
                .HasColumnType("date")
                .IsRequired();

            // CONSULTANT POSITIONS ACCOUNTING CONFIGURATION
            modelBuilder.Entity<ConsultantPositionAccountingConfiguration>(entity =>
            {
                entity.HasIndex(e => e.CostCenterId);
                entity.HasIndex(e => e.AccountingAccountId);
                entity.HasIndex(e => e.MovementTypeId);
                entity.HasIndex(e => e.PositionId);
            });
            modelBuilder.Entity<ConsultantPositionAccountingConfiguration>()
                .HasKey(r => new { r.Id });
            modelBuilder.Entity<ConsultantPositionAccountingConfiguration>()
                .HasOne(c => c.CostCenter)
                .WithMany()
                .HasForeignKey(c => c.CostCenterId)
                .IsRequired();
            modelBuilder.Entity<ConsultantPositionAccountingConfiguration>()
                .HasOne(a => a.AccountingAccount)
                .WithMany()
                .HasForeignKey(a => a.AccountingAccountId)
                .IsRequired();
            modelBuilder.Entity<ConsultantPositionAccountingConfiguration>()
                .HasOne(r => r.ConsultantPosition)
                .WithMany()
                .HasForeignKey(r => r.PositionId)
                .IsRequired();
            modelBuilder.Entity<ConsultantPositionAccountingConfiguration>()
               .HasOne(m => m.MovementType)
               .WithMany()
               .HasForeignKey(m => m.MovementTypeId)
               .IsRequired();
            modelBuilder.Entity<ConsultantPositionAccountingConfiguration>()
                .Property(d => d.CompanyId)
                .HasColumnType("varchar")
                .IsRequired();

            // REPORTING MY TIME MOVEMENTS
            modelBuilder.Entity<ReportingMyTimeMovement>(entity =>
            {
                entity.HasIndex(e => new { e.ProjectId, e.ConsultantId, e.ActionDate });
                entity.HasIndex(e => new { e.ProjectId, e.ConsultantId });

                entity.HasIndex(e => e.ActionDate);
                entity.HasIndex(e => e.Quantity);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.TransactionStatusId);
                entity.HasIndex(e => e.MovementTypeId);
            });
            modelBuilder.Entity<ReportingMyTimeMovement>()
                .HasKey(r => new { r.MovementId });
            modelBuilder.Entity<ReportingMyTimeMovement>()
                .HasOne(p => p.Project)
                .WithMany()
                .HasForeignKey(p => p.ProjectId)
                .IsRequired();
            modelBuilder.Entity<ReportingMyTimeMovement>()
                .HasOne(c => c.ConsultantDetail)
                .WithMany()
                .HasForeignKey(c => c.ConsultantId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ReportingMyTimeMovement>()
                .HasOne(t => t.TransactionStatus)
                .WithMany()
                .HasForeignKey(t => t.TransactionStatusId)
                .IsRequired();
            modelBuilder.Entity<ReportingMyTimeMovement>()
                .HasOne(r => r.ReportingMyTimeMovementType)
                .WithMany()
                .HasForeignKey(r => r.MovementTypeId)
                .IsRequired();
            modelBuilder.Entity<ReportingMyTimeMovement>()
                .Property(d => d.ActionDate)
                .HasColumnType("date")
                .IsRequired();
            modelBuilder.Entity<ReportingMyTimeMovement>()
                .Property(d => d.TimeFrom)
                .HasColumnType("varchar");
            modelBuilder.Entity<ReportingMyTimeMovement>()
                .Property(d => d.TimeTo)
                .HasColumnType("varchar");

            // REPORTING MY TIME MOVEMENT BLOBS
            modelBuilder.Entity<ReportingMyTimeMovementBlob>(entity =>
            {
                entity.HasIndex(e => e.MovementId);
            });
            modelBuilder.Entity<ReportingMyTimeMovementBlob>()
                .HasKey(r => new { r.InternalBlobId });
            modelBuilder.Entity<ReportingMyTimeMovementBlob>()
                .HasOne(p => p.ReportingMyTimeMovement)
                .WithMany()
                .HasForeignKey(p => p.MovementId)
                .IsRequired();
            modelBuilder.Entity<ReportingMyTimeMovementBlob>()
                .Property(c => c.ContainerId)
                .HasColumnType("VARCHAR(255)");
            modelBuilder.Entity<ReportingMyTimeMovementBlob>()
                .Property(c => c.ContentType)
                .HasColumnType("VARCHAR(255)");

            // REPORTING MY TIME SUBMISSIONS
            modelBuilder.Entity<ReportingMyTimeMovementSubmission>(entity =>
            {
                entity.HasIndex(e => new { e.ProjectId, e.ConsultantId });

                entity.HasIndex(e => e.SubmissionId);
                entity.HasIndex(e => e.SubmissionDate);
                entity.HasIndex(e => e.LastSubmissionDate);
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.StartPeriodDate);
                entity.HasIndex(e => e.EndPeriodDate);
                entity.HasIndex(e => e.TransactionStatusId);
            });
            modelBuilder.Entity<ReportingMyTimeMovementSubmission>(entity =>
            {
                // Composite index
                entity.HasIndex(e => new { e.ConsultantId, e.ProjectId, e.StartPeriodDate, e.EndPeriodDate });
                entity.HasIndex(e => new { e.TransactionStatusId, e.StartPeriodDate, e.EndPeriodDate });

                // Indexes on foreign keys
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.TransactionStatusId);
                entity.HasIndex(e => e.SubmissionDate);
                entity.HasIndex(e => e.LastSubmissionDate);
            });
            modelBuilder.Entity<ReportingMyTimeMovementSubmission>()
                .HasKey(r => new { r.SubmissionId });
            modelBuilder.Entity<ReportingMyTimeMovementSubmission>()
                .HasOne(p => p.Project)
                .WithMany()
                .HasForeignKey(p => p.ProjectId)
                .IsRequired();
            modelBuilder.Entity<ReportingMyTimeMovementSubmission>()
                .HasOne(c => c.ConsultantDetail)
                .WithMany()
                .HasForeignKey(c => c.ConsultantId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ReportingMyTimeMovementSubmission>()
                .HasOne(t => t.TransactionStatus)
                .WithMany()
                .HasForeignKey(t => t.TransactionStatusId)
                .IsRequired();
            modelBuilder.Entity<ReportingMyTimeMovementSubmission>()
                .Property(d => d.StartPeriodDate)
                .HasColumnType("date")
                .IsRequired();
            modelBuilder.Entity<ReportingMyTimeMovementSubmission>()
                .Property(d => d.EndPeriodDate)
                .HasColumnType("date")
                .IsRequired();

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
                entity.HasIndex(e => e.TransactionStatusId);
            });

            // CONSULTANTS BENEFITS
            // REPORTING MY TIME SUBMISSIONS
            modelBuilder.Entity<ConsultantBenefit>(entity =>
            {
                // Índices en fechas
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Amount);
                entity.HasIndex(e => e.BenefitPeriod);
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.EndDate);
            });
            modelBuilder.Entity<ConsultantBenefit>()
                .HasKey(c => new { c.BenefitId });

            // CONSULTANTS BENEFITS COMPANIES
            modelBuilder.Entity<ConsultantBenefitCompany>(entity =>
            {
                // Indexes on foreign keys
                entity.HasIndex(e => e.CostCenterId);
                entity.HasIndex(e => e.AccountingAccountId);
                entity.HasIndex(e => e.BenefitId);

                // Indexes for columns
                entity.HasIndex(e => e.CompanyId);
            });
            modelBuilder.Entity<ConsultantBenefitCompany>()
                .HasKey(c => new { c.ConsultantaBenefitCompanyId });
            modelBuilder.Entity<ConsultantBenefitCompany>()
                .HasOne(cc => cc.ConsultantBenefit)
                .WithMany()
                .HasForeignKey(cc => cc.BenefitId)
                .IsRequired();
            modelBuilder.Entity<ConsultantBenefitCompany>()
                .HasOne(cc => cc.CostCenter)
                .WithMany()
                .HasForeignKey(cc => cc.CostCenterId)
                .IsRequired();
            modelBuilder.Entity<ConsultantBenefitCompany>()
                .HasOne(cc => cc.AccountingAccount)
                .WithMany()
                .HasForeignKey(cc => cc.AccountingAccountId)
                .IsRequired();

            // CONSULTANTS BENEFITS CATEGORIES
            modelBuilder.Entity<ConsultantBenefitCategory>(entity =>
            {
                // Indexes on foreign keys
                entity.HasIndex(e => e.BenefitId);

                // Indexes for columns
                entity.HasIndex(e => e.Name);
            });
            modelBuilder.Entity<ConsultantBenefitCategory>()
                .HasKey(c => new { c.BenefitCategoryId });
            modelBuilder.Entity<ConsultantBenefitCategory>()
                .HasOne(cb => cb.ConsultantBenefit)
                .WithMany()
                .HasForeignKey(cb => cb.BenefitId)
                .IsRequired();

            // CONSULTANTS REIMBURSED BENEFITS
            modelBuilder.Entity<ConsultantReimbursedBenefit>(entity =>
            {
                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.BenefitId);
                entity.HasIndex(e => e.AmountReimbursed);
                entity.HasIndex(e => e.DateToBeReimbursed);
                entity.HasIndex(e => e.ConsultantIdCreatedBy);
                entity.HasIndex(e => e.ConsultantIdLastUpdatedBy);
                entity.HasIndex(e => e.BenefitCategoryId);
                entity.HasIndex(e => e.TransactionStatusId);
            });
            modelBuilder.Entity<ConsultantReimbursedBenefit>()
                .HasKey(c => new { c.ReimbursedBenefitId });
            modelBuilder.Entity<ConsultantReimbursedBenefit>()
                .HasOne(cb => cb.ConsultantBenefit)
                .WithMany()
                .HasForeignKey(cb => cb.BenefitId)
                .IsRequired();
            modelBuilder.Entity<ConsultantReimbursedBenefit>()
                .HasOne(cb => cb.ConsultantDetailBenefit)
                .WithMany()
                .HasForeignKey(cb => cb.ConsultantId)
                .IsRequired();
            modelBuilder.Entity<ConsultantReimbursedBenefit>()
                .HasOne(cb => cb.ConsultantDetailCreatedBy)
                .WithMany()
                .HasForeignKey(cb => cb.ConsultantIdCreatedBy)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ConsultantReimbursedBenefit>()
                .HasOne(cb => cb.ConsultantDetailUpdatedBy)
                .WithMany()
                .HasForeignKey(cb => cb.ConsultantIdLastUpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ConsultantReimbursedBenefit>()
                .HasOne(cc => cc.ConsultantBenefitCategory)
                .WithMany()
                .HasForeignKey(cc => cc.BenefitCategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ConsultantReimbursedBenefit>()
                .HasOne(cc => cc.TransactionStatus)
                .WithMany()
                .HasForeignKey(cc => cc.TransactionStatusId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ConsultantReimbursedBenefit>()
                .Property(d => d.DateToBeReimbursed)
                .HasColumnType("date")
                .IsRequired();

            // CONSULTANT PAYMENTS DEBITS AND CREDITS
            modelBuilder.Entity<ConsultantPaymentDebitsCredits>(entity =>
            {
                entity.HasIndex(e => e.ConsultantId);
                entity.HasIndex(e => e.AccountingAccountId);
                entity.HasIndex(e => e.CostCenterId);
                entity.HasIndex(e => e.TransactionStatusId);
                entity.HasIndex(e => e.TransactionTypeId);
                entity.HasIndex(e => e.ConsultantIdCreatedBy);
                entity.HasIndex(e => e.ConsultantIdLastUpdatedBy);
                entity.HasIndex(e => e.Quantity);
                entity.HasIndex(e => e.Amount);
            });
            modelBuilder.Entity<ConsultantPaymentDebitsCredits>()
                .HasKey(c => new { c.ConsultantPaymentDebitsCreditsId });
            modelBuilder.Entity<ConsultantPaymentDebitsCredits>()
                .HasOne(cp => cp.ConsultantDetail)
                .WithMany()
                .HasForeignKey(cp => cp.ConsultantId)
                .IsRequired();
            modelBuilder.Entity<ConsultantPaymentDebitsCredits>()
                .HasOne(cp => cp.AccountingAccount)
                .WithMany()
                .HasForeignKey(cp => cp.AccountingAccountId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ConsultantPaymentDebitsCredits>()
                .HasOne(cp => cp.CostCenter)
                .WithMany()
                .HasForeignKey(cp => cp.CostCenterId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ConsultantPaymentDebitsCredits>()
                .HasOne(cp => cp.TransactionStatus)
                .WithMany()
                .HasForeignKey(cp => cp.TransactionStatusId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ConsultantPaymentDebitsCredits>()
                .HasOne(cp => cp.TransactionType)
                .WithMany()
                .HasForeignKey(cp => cp.TransactionTypeId)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ConsultantPaymentDebitsCredits>()
                .HasOne(cp => cp.ConsultantDetailCreatedBy)
                .WithMany()
                .HasForeignKey(cp => cp.ConsultantIdCreatedBy)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ConsultantPaymentDebitsCredits>()
                .HasOne(cp => cp.ConsultantDetailUpdatedBy)
                .WithMany()
                .HasForeignKey(cp => cp.ConsultantIdLastUpdatedBy);
            modelBuilder.Entity<ConsultantPaymentDebitsCredits>()
                .Property(d => d.ActionDateWithinFortnight)
                .HasColumnType("date")
                .IsRequired();
        }
        public DbSet<AccountingAccount> ACCOUNTING_ACCOUNT { get; set; }
        public DbSet<LedgerMovement> LEDGER_MOVEMENT { get; set; }
        public DbSet<DataUpdateDate> DATA_UPDATE_DATES { get; set; }
        public DbSet<ApplicationUser> AspNetUsers { get; set; }
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
        public DbSet<Company> COMPANIES { get; set; }
        public DbSet<ProviderCategory> PROVIDER_CATEGORY { get; set; }
        public DbSet<Provider> PROVIDER { get; set; }
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
        public DbSet<ConsultantBenefitCompany> CONSULTANT_BENEFIT_COMPANIES { get; set; }
        public DbSet<ConsultantBenefitCategory> CONSULTANT_BENEFIT_CATEGORIES { get; set; }
        public DbSet<ConsultantPaymentDebitsCredits> CONSULTANT_PAYMENTS_DEBITS_CREDITS { get; set; }
        public DbSet<ConsultantPositionAccountingConfiguration> CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION { get; set; }
        public DbSet<ConsultantReimbursedBenefit> CONSULTANT_REIMBURSED_BENEFITS { get; set; }
        public DbSet<Interview> INTERVIEWS { get; set; }
        public DbSet<Partner> PARTNERS { get; set; }
        public DbSet<PaymentMethod> PAYMENT_METHODS { get; set; }
        public DbSet<Project> PROJECTS { get; set; }
        public DbSet<ProjectConsultantAssigned> PROJECTS_CONSULTANTS_ASSIGNED { get; set; }
        public DbSet<ProjectConsultantAssignedHistory> PROJECTS_CONSULTANTS_ASSIGNED_HISTORY { get; set; }
        public DbSet<ProjectConsultantPeriodDisabledTracking> PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS { get; set; }
        public DbSet<ProjectUserSelected> PROJECTS_USERS_SELECTED { get; set; }
        public DbSet<ProviderEvent> PROVIDER_EVENTS { get; set; }
        public DbSet<ProviderEventDate> PROVIDER_EVENT_DATES { get; set; }
        public DbSet<ReportingMyTimeComments> REPORTING_MY_TIME_COMMENTS { get; set; }
        public DbSet<ReportingMyTimeMovement> REPORTING_MY_TIME_MOVEMENTS { get; set; }
        public DbSet<ReportingMyTimeMovementBlob> REPORTING_MY_TIME_MOVEMENT_BLOBS { get; set; }
        public DbSet<ReportingMyTimeMovementType> REPORTING_MY_TIME_MOVEMENT_TYPES { get; set; }
        public DbSet<ReportingMyTimeMovementSubmission> REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS { get; set; }
        public DbSet<TransactionType> TRANSACTION_TYPES { get; set; }
        public DbSet<TransactionStatus> TRANSACTION_STATUSES { get; set; }
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
