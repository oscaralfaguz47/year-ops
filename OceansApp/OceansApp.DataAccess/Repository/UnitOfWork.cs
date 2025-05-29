
using Azure.Storage.Queues;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;

namespace OceansApp.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMemoryCache _cache;
        private readonly IProjectConsultantAssignedHistoryRepository _projectConsultantAssignedHistoryRepository;
        private readonly TelemetryClient _telemetryClient;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly Lazy<QueueClient> _queueClient;
        public UnitOfWork(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IMemoryCache cache, 
            IProjectConsultantAssignedHistoryRepository projectConsultantAssignedHistoryRepository, TelemetryClient telemetryClient,
            HttpClient httpClient, IConfiguration config, Lazy<QueueClient> queueClient)
        {
            ProjectConsultantAssignedHistory = projectConsultantAssignedHistoryRepository;
            _telemetryClient = telemetryClient;
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _cache = cache;
            _httpClient = httpClient;
            _config = config;
            _queueClient = queueClient;
            AccountingAccounts = new AccountingAccountRepository(_db);
            AccountPayable = new AccountPayableRepository(_db);
            AccountPayableMovement = new AccountPayableMovementRepository(_db);
            Bonusly = new BonuslyRepository(_httpClient, _config);
            CenterOfCosts = new CostCenterRepository(_db);
            LedgerMovements = new LedgerMovementRepository(_db);
            DataUpdateDates = new DataUpdateRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            ApplicationUserActiveHistory = new ApplicationUserActiveHistoryRepository(_db);
            ApplicationUserCategory = new ApplicationUserCategoryRepository(_db);
            ApplicationRoleClaim = new ApplicationRoleClaimRepository(_db, _userManager, _roleManager);
            ApplicationSystemClaim = new ApplicationSystemClaimRepository(_db);
            BankAccount = new BankAccountRepository(_db);
            CalculatorGlobalConfiguration = new CalculatorGlobalConfigurationRepository(_db);
            CalculatorCostCenterIncreaseConfiguration = new CalculatorCostCenterIncreaseConfigurationRepository(_db);
            CalculatorSearchHistory = new CalculatorSearchHistoryRepository(_db);
            CalculatorAccountingAccountToIgnore = new CalculatorAccountingAccountToIgnoreRepository(_db);
            Client = new ClientRepository(_db);
            Country = new CountryRepository(_db);
            Company = new CompanyRepository(_db);
            ConsultantDetail = new ConsultantDetailRepository(_db, _userManager, _cache);
            ConsultantPayment = new ConsultantPaymentRepository(_db, this, _config, _queueClient);
            ConsultantPaymentsDebitsCredits = new ConsultantPaymentDebitsCreditsRepository(_db, this);
            ConsultantPosition = new ConsultantPositionRepository(_db);
            ConsultantBenefit = new ConsultantBenefitRepository(_db);
            ConsultantAndBenefit = new ConsultantAndBenefitRepository(_db);
            ConsultantBenefitCategory = new ConsultantBenefitCategoryRepository(_db);
            ConsultantReimbursedBenefit = new ConsultantReimbursedBenefitRepository(_db, this);
            ConsultantHoliday = new ConsultantHolidayRepository(_db);
            ConsultantRole = new ConsultantRoleRepository(_db);
            ConsultantQualityLevel = new ConsultantQualityLevelRepository(_db);
            ConsultantRoleQualityLevel = new ConsultantRoleQualityLevelRepository(_db);
            ConsultantSeniority = new ConsultantSeniorityRepository(_db);
            CostCenterAccountingAccount = new CostCenterAccountingAccountRepository(_db);
            DocumentCCSubtype = new DocumentCCSubtypeRepository(_db);
            DocumentType = new DocumentTypeRepository(_db);
            Interview = new InterviewRepository(_db, this);
            ImageBlob = new ImageBlobRepository(_db);
            JournalAccountPayable = new JournalAccountPayableRepository(_db);
            JournalAccountPayableEntry = new JournalAccountPayableEntryRepository(_db);
            Partner = new PartnerRepository(_db);
            PaymentMethod = new PaymentMethodRepository(_db);
            PaymentBookEntryParent = new PaymentBookEntryParentRepository(_db);
            Product = new ProductRepository(_db);
            ProductClientCompanyAccountingConfig = new ProductClientCompanyAccountingConfigForBillingRepository(_db);
            Project = new ProjectRepository(_db);
            ProjectConsultantAssigned = new ProjectConsultantAssignedRepository(_db);
            ProjectConsultantPeriodDisabledTracking = new ProjectConsultantPeriodDisabledTrackingRepository(_db);
            ProjectConsultantAssignedHistory = new ProjectConsultantAssignedHistoryRepository(_db);
            ProjectConsultantPendingSubmission = new ProjectConsultantPendingSubmissionRepository(_db, _telemetryClient);
            ProjectUserSelected = new ProjectUserSelectedRepository(_db);
            ReportingMyTimeMovement = new ReportingMyTimeMovementRepository(_db, this);
            ReportingMyTimeMovementSubmission = new ReportingMyTimeMovementSubmissionRepository(_db);
            ReportingMyTimeMovementType = new ReportingMyTimeMovementTypeRepository(_db);
            DocumentCC = new DocumentCCRepository(_db);
            DocumentsCCNotification = new DocumentsCCNotificationRepository(_db);
            NotificationType = new NotificationTypeRepository(_db);
            Notification = new NotificationRepository(_db);
            NotificationStatus = new NotificationStatusRepository(_db);
            NotificationMedia = new NotificationMediaRepository(_db);
            NotificationRecipient = new NotificationRecipientRepository(_db);
            SystemArea = new SystemAreaRepository(_db);
            SystemSubArea = new SystemSubAreaRepository(_db);
            TransactionStatus = new TransactionStatusRepository(_db);
        }
        public IApplicationSystemClaimRepository ApplicationSystemClaim { get; private set; }
        public IAccountingAccountRepository AccountingAccounts { get; private set; }
        public IAccountPayableRepository AccountPayable { get; private set; }
        public IAccountPayableMovementRepository AccountPayableMovement { get; private set; }
        public IBonuslyRepository Bonusly { get; private set; }
        public ICostCenterRepository CenterOfCosts { get; private set; }
        public ILedgerMovementRepository LedgerMovements { get; private set; }
        public IDataUpdateDateRepository DataUpdateDates { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IApplicationUserActiveHistoryRepository ApplicationUserActiveHistory { get; private set; }
        public IApplicationUserCategoryRepository ApplicationUserCategory { get; private set; }
        public IApplicationRoleClaimRepository ApplicationRoleClaim { get; private set; }
        public IBankAccountRepository BankAccount { get; private set; }
        public ICalculatorGlobalConfigurationRepository CalculatorGlobalConfiguration { get; set; }
        public ICalculatorCostCenterIncreaseConfigurationRepository CalculatorCostCenterIncreaseConfiguration { get; set; }
        public ICalculatorSearchHistoryRepository CalculatorSearchHistory { get; set; }
        public ICalculatorAccountingAccountToIgnoreRepository CalculatorAccountingAccountToIgnore { get; set; }
        public IClientRepository Client { get; set; }
        public IPartnerRepository Partner { get; set; }
        public ICountryRepository Country { get; set; }
        public ICompanyRepository Company { get; set; }
        public IConsultantHolidayRepository ConsultantHoliday { get; set; }
        public IConsultantDetailRepository ConsultantDetail { get; set; }
        public IConsultantPaymentRepository ConsultantPayment{ get; set; }
        public IConsultantPaymentDebitsCreditsRepository ConsultantPaymentsDebitsCredits { get; set; }
        public IConsultantPositionRepository ConsultantPosition { get; set; }
        public IConsultantBenefitRepository ConsultantBenefit { get; set; }
        public IConsultantAndBenefitRepository ConsultantAndBenefit { get; set; }
        public IConsultantBenefitCategoryRepository ConsultantBenefitCategory { get; set; }
        public IConsultantReimbursedBenefitRepository ConsultantReimbursedBenefit { get; set; }
        public IConsultantRoleRepository ConsultantRole { get; set; }
        public IConsultantQualityLevelRepository ConsultantQualityLevel { get; set; }
        public IConsultantRoleQualityLevelRepository ConsultantRoleQualityLevel { get; set; }
        public IConsultantSeniorityRepository ConsultantSeniority { get; set; }
        public ICostCenterAccountingAccountRepository CostCenterAccountingAccount { get; set; }
        public IDocumentCCSubtypeRepository DocumentCCSubtype { get; set; }
        public IDocumentTypeRepository DocumentType { get; set; }
        public IInterviewRepository Interview { get; set; }
        public IImageBlobRepository ImageBlob { get; set; }
        public IJournalAccountPayableRepository JournalAccountPayable { get; set; }
        public IJournalAccountPayableEntryRepository JournalAccountPayableEntry { get; set; }
        public IPaymentMethodRepository PaymentMethod { get; set; }
        public IPaymentBookEntryParentRepository PaymentBookEntryParent { get; set; }
        public IProductRepository Product { get; set; }
        public IProductClientCompanyAccountingConfigForBillingRepository ProductClientCompanyAccountingConfig { get; set; }
        public IProjectRepository Project { get; set; }
        public IProjectConsultantAssignedRepository ProjectConsultantAssigned { get; set; }
        public IProjectConsultantPeriodDisabledTrackingRepository ProjectConsultantPeriodDisabledTracking { get; set; }
        public IProjectConsultantAssignedHistoryRepository ProjectConsultantAssignedHistory { get; set; }
        public IProjectConsultantPendingSubmissionRepository ProjectConsultantPendingSubmission { get; set; }
        public IProjectUserSelectedRepository ProjectUserSelected { get; set; }
        public IReportingMyTimeMovementRepository ReportingMyTimeMovement { get; set; }
        public IReportingMyTimeMovementSubmissionRepository ReportingMyTimeMovementSubmission { get; set; }
        public IReportingMyTimeMovementTypeRepository ReportingMyTimeMovementType { get; set; }
        public IDocumentCCRepository DocumentCC { get; set; }
        public IDocumentsCCNotificationRepository DocumentsCCNotification { get; set; }
        public INotificationTypeRepository NotificationType { get; set; }
        public INotificationRepository Notification { get; set; }
        public INotificationStatusRepository NotificationStatus { get; set; }
        public INotificationMediaRepository NotificationMedia { get; set; }
        public INotificationRecipientRepository NotificationRecipient { get; set; }
        public ISystemAreaRepository SystemArea { get; set; }
        public ISystemSubAreaRepository SystemSubArea { get; set; }
        public ITransactionStatusRepository TransactionStatus { get; set; }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
        public Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTranAsync()
        {
          return _db.Database.BeginTransactionAsync();
        }
    }
}
