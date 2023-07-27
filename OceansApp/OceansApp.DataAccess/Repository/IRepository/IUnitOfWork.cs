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
        IConsultantRoleRepository ConsultantRole { get; }
        IConsultantQualityLevelRepository ConsultantQualityLevel { get; }
        IConsultantRoleQualityLevelRepository ConsultantRoleQualityLevel { get; }
        IProviderEventRepository ProviderEvent { get; }
        IProviderEventDateRepository ProviderEventDate { get; }
        IDocumentCCRepository DocumentCC { get; }
        INotificationTypeRepository NotificationType { get; }

        void Save();
    }
}
