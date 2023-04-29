

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


        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
