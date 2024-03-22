
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;

namespace OceansApp.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly UserManager<IdentityUser> _userManager;
        public UnitOfWork(ApplicationDbContext db, IConfiguration config, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _config = config;
            _userManager = userManager;
            AccountingAccounts = new AccountingAccountRepository(_db);
            CenterOfCosts = new CostCenterRepository(_db);
            LedgerMovements = new LedgerMovementRepository(_db);
            DataUpdateDates = new DataUpdateRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            ApplicationUserCategory = new ApplicationUserCategoryRepository(_db);
            ApplicationRoleClaim = new ApplicationRoleClaimRepository(_db);
            ApplicationSystemClaim = new ApplicationSystemClaimRepository(_db);
            CalculatorGlobalConfiguration = new CalculatorGlobalConfigurationRepository(_db);
            CalculatorCostCenterIncreaseConfiguration = new CalculatorCostCenterIncreaseConfigurationRepository(_db);
            CalculatorSearchHistory = new CalculatorSearchHistoryRepository(_db);
            CalculatorAccountingAccountToIgnore = new CalculatorAccountingAccountToIgnoreRepository(_db);
            Client = new ClientRepository(_db);
            ProviderCategory = new ProviderCategoryRepository(_db);
            Provider = new ProviderRepository(_db);
            Country = new CountryRepository(_db);
            ConsultantDetail = new ConsultantDetailRepository(_db, _config, _userManager);
            ConsultantPaymentsDebitsCredits = new ConsultantPaymentDebitsCreditsRepository(_db);
            ConsultantPosition = new ConsultantPositionRepository(_db);
            ConsultantBenefit = new ConsultantBenefitRepository(_db);
            ConsultantBenefitCategory = new ConsultantBenefitCategoryRepository(_db);
            ConsultantReimbursedBenefit = new ConsultantReimbursedBenefitRepository(_db);
            ConsultantHoliday = new ConsultantHolidayRepository(_db);
            ConsultantRole = new ConsultantRoleRepository(_db);
            ConsultantQualityLevel = new ConsultantQualityLevelRepository(_db);
            ConsultantRoleQualityLevel = new ConsultantRoleQualityLevelRepository(_db);
            ConsultantSeniority = new ConsultantSeniorityRepository(_db);
            CostCenterAccountingAccount = new CostCenterAccountingAccountRepository(_db);
            PaymentMethod = new PaymentMethodRepository(_db);
            Project = new ProjectRepository(_db);
            ProjectConsultantAssigned = new ProjectConsultantAssignedRepository(_db);
            ProjectConsultantAssignedHistory = new ProjectConsultantAssignedHistoryRepository(_db);
            ProjectConsultantAssignedHistoryAction = new ProjectConsultantAssignedHistoryActionRepository(_db);
            ProviderEvent = new ProviderEventRepository(_db);
            ProviderEventDate = new ProviderEventDateRepository(_db);
            DocumentCC = new DocumentCCRepository(_db);
            DocumentsCCNotification = new DocumentsCCNotificationRepository(_db);
            NotificationType = new NotificationTypeRepository(_db);
            Notification = new NotificationRepository(_db);
            NotificationStatus = new NotificationStatusRepository(_db);
            NotificationMedia = new NotificationMediaRepository(_db);
            NotificationRecipient = new NotificationRecipientRepository(_db);
            SystemArea = new SystemAreaRepository(_db);
            SystemSubArea = new SystemSubAreaRepository(_db);
        }
        public IApplicationSystemClaimRepository ApplicationSystemClaim { get; private set; }
        public IAccountingAccountRepository AccountingAccounts { get; private set; }
        public ICostCenterRepository CenterOfCosts { get; private set; }
        public ILedgerMovementRepository LedgerMovements { get; private set; }
        public IDataUpdateDateRepository DataUpdateDates { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IApplicationUserCategoryRepository ApplicationUserCategory { get; private set; }
        public IApplicationRoleClaimRepository ApplicationRoleClaim { get; private set; }
        public ICalculatorGlobalConfigurationRepository CalculatorGlobalConfiguration { get; set; }
        public ICalculatorCostCenterIncreaseConfigurationRepository CalculatorCostCenterIncreaseConfiguration { get; set; }
        public ICalculatorSearchHistoryRepository CalculatorSearchHistory { get; set; }
        public ICalculatorAccountingAccountToIgnoreRepository CalculatorAccountingAccountToIgnore { get; set; }
        public IClientRepository Client { get; set; }
        public IProviderCategoryRepository ProviderCategory { get; set; }
        public IProviderRepository Provider { get; set; }
        public ICountryRepository Country { get; set; }
        public IConsultantHolidayRepository ConsultantHoliday { get; set; }
        public IConsultantDetailRepository ConsultantDetail { get; set; }
        public IConsultantPaymentDebitsCreditsRepository ConsultantPaymentsDebitsCredits { get; set; }
        public IConsultantPositionRepository ConsultantPosition { get; set; }
        public IConsultantBenefitRepository ConsultantBenefit { get; set; }
        public IConsultantBenefitCategoryRepository ConsultantBenefitCategory { get; set; }
        public IConsultantReimbursedBenefitRepository ConsultantReimbursedBenefit { get; set; }
        public IConsultantRoleRepository ConsultantRole { get; set; }
        public IConsultantQualityLevelRepository ConsultantQualityLevel { get; set; }
        public IConsultantRoleQualityLevelRepository ConsultantRoleQualityLevel { get; set; }
        public IConsultantSeniorityRepository ConsultantSeniority { get; set; }
        public ICostCenterAccountingAccountRepository CostCenterAccountingAccount { get; set; }
        public IPaymentMethodRepository PaymentMethod { get; set; }
        public IProjectRepository Project { get; set; }
        public IProjectConsultantAssignedRepository ProjectConsultantAssigned { get; set; }
        public IProjectConsultantAssignedHistoryRepository ProjectConsultantAssignedHistory { get; set; }
        public IProjectConsultantAssignedHistoryActionRepository ProjectConsultantAssignedHistoryAction { get; set; }
        public IProviderEventRepository ProviderEvent { get; set; }
        public IProviderEventDateRepository ProviderEventDate { get; set; }
        public IDocumentCCRepository DocumentCC { get; set; }
        public IDocumentsCCNotificationRepository DocumentsCCNotification { get; set; }
        public INotificationTypeRepository NotificationType { get; set; }
        public INotificationRepository Notification { get; set; }
        public INotificationStatusRepository NotificationStatus { get; set; }
        public INotificationMediaRepository NotificationMedia { get; set; }
        public INotificationRecipientRepository NotificationRecipient { get; set; }
        public ISystemAreaRepository SystemArea { get; set; }
        public ISystemSubAreaRepository SystemSubArea { get; set; }

        public void Save()
        {
            _db.SaveChanges();
        }
        public Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTran()
        {
          return _db.Database.BeginTransactionAsync();
        }
    }
}
