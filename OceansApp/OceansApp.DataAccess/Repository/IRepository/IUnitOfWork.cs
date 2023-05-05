namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IAccountingAccountRepository AccountingAccounts{ get; }
        ICostCenterRepository CenterOfCosts { get; }

        ILedgerMovementRepository LedgerMovements { get; }
        IDataUpdateDateRepository DataUpdateDates { get; }
        IApplicationUserRepository ApplicationUser { get; }
        ICalculatorGlobalConfigurationRepository CalculatorGlobalConfiguration { get; }
        ICalculatorCostCenterIncreaseConfigurationRepository CalculatorCostCenterIncreaseConfiguration { get; }
        ICalculatorSearchHistoryRepository CalculatorSearchHistory { get; }
        ICalculatorAccountingAccountToIgnoreRepository CalculatorAccountingAccountToIgnore { get; }
        IClientRepository Client { get; }
        IProviderCategoryRepository ProviderCategory { get; }
        IProviderRepository Provider { get; }
        ICountryRepository Country { get; }

        void Save();
    }
}
