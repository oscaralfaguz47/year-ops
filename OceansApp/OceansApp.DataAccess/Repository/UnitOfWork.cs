

using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;

namespace OceansApp.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            AccountingAccounts = new AccountingAccountRepository(_db);
            CenterOfCosts = new CostCenterRepository(_db);
            LedgerMovements = new LedgerMovementRepository(_db);
            DataUpdateDates = new DataUpdateRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            CalculatorGlobalConfiguration = new CalculatorGlobalConfigurationRepository(_db);
            CalculatorCostCenterIncreaseConfiguration = new CalculatorCostCenterIncreaseConfigurationRepository(_db);
            CalculatorSearchHistory = new CalculatorSearchHistoryRepository(_db);
            CalculatorAccountingAccountToIgnore = new CalculatorAccountingAccountToIgnoreRepository(_db);
            Client = new ClientRepository(_db);
            ProviderCategory = new ProviderCategoryRepository(_db);
            Provider = new ProviderRepository(_db);
            Country = new CountryRepository(_db);
            ConsultantRole = new ConsultantRoleRepository(_db);
            ConsultantQualityLevel = new ConsultantQualityLevelRepository(_db);
            ConsultantRoleQualityLevel = new ConsultantRoleQualityLevelRepository(_db);
            ProviderEvent = new ProviderEventRepository(_db);
            ProviderEventDate = new ProviderEventDateRepository(_db);
            DocumentCC = new DocumentCCRepository(_db);
            NotificationType = new NotificationTypeRepository(_db);
            Notification = new NotificationRepository(_db);
            NotificationStatus = new NotificationStatusRepository(_db);
            NotificationMedia = new NotificationMediaRepository(_db);
            NotificationRecipient = new NotificationRecipientRepository(_db);
        }
        public IAccountingAccountRepository AccountingAccounts { get; private set; }
        public ICostCenterRepository CenterOfCosts { get; private set; }
        public ILedgerMovementRepository LedgerMovements { get; private set; }
        public IDataUpdateDateRepository DataUpdateDates { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public ICalculatorGlobalConfigurationRepository CalculatorGlobalConfiguration { get; set; }
        public ICalculatorCostCenterIncreaseConfigurationRepository CalculatorCostCenterIncreaseConfiguration { get; set; }
        public ICalculatorSearchHistoryRepository CalculatorSearchHistory { get; set; }
        public ICalculatorAccountingAccountToIgnoreRepository CalculatorAccountingAccountToIgnore { get; set; }
        public IClientRepository Client { get; set; }
        public IProviderCategoryRepository ProviderCategory { get; set; }
        public IProviderRepository Provider { get; set; }
        public ICountryRepository Country { get; set; }
        public IConsultantRoleRepository ConsultantRole { get; set; }
        public IConsultantQualityLevelRepository ConsultantQualityLevel { get; set; }
        public IConsultantRoleQualityLevelRepository ConsultantRoleQualityLevel { get; set; }
        public IProviderEventRepository ProviderEvent { get; set; }
        public IProviderEventDateRepository ProviderEventDate { get; set; }
        public IDocumentCCRepository DocumentCC { get; set; }
        public INotificationTypeRepository NotificationType { get; set; }
        public INotificationRepository Notification { get; set; }
        public INotificationStatusRepository NotificationStatus { get; set; }
        public INotificationMediaRepository NotificationMedia { get; set; }
        public INotificationRecipientRepository NotificationRecipient { get; set; }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
