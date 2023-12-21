namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IAccountingAccountRepository AccountingAccounts{ get; }
        ICostCenterRepository CenterOfCosts { get; }
        ILedgerMovementRepository LedgerMovements { get; }
        IDataUpdateDateRepository DataUpdateDates { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IApplicationRoleClaimRepository ApplicationRoleClaim { get; }
        IApplicationSystemClaimRepository ApplicationSystemClaim { get; }
        ICalculatorGlobalConfigurationRepository CalculatorGlobalConfiguration { get; }
        ICalculatorCostCenterIncreaseConfigurationRepository CalculatorCostCenterIncreaseConfiguration { get; }
        ICalculatorSearchHistoryRepository CalculatorSearchHistory { get; }
        ICalculatorAccountingAccountToIgnoreRepository CalculatorAccountingAccountToIgnore { get; }
        IClientRepository Client { get; }
        IProviderCategoryRepository ProviderCategory { get; }
        IProviderRepository Provider { get; }
        ICountryRepository Country { get; }
        IConsultantHolidayRepository ConsultantHoliday { get; }
        IConsultantRoleRepository ConsultantRole { get; }
        IConsultantQualityLevelRepository ConsultantQualityLevel { get; }
        IConsultantRoleQualityLevelRepository ConsultantRoleQualityLevel { get; }
        IConsultantSeniorityRepository ConsultantSeniority { get; }
        IProviderEventRepository ProviderEvent { get; }
        IProviderEventDateRepository ProviderEventDate { get; }
        IDocumentCCRepository DocumentCC { get; }
        IDocumentsCCNotificationRepository DocumentsCCNotification { get; }
        INotificationTypeRepository NotificationType { get; }
        INotificationRepository Notification { get; }
        INotificationStatusRepository NotificationStatus { get; }
        INotificationMediaRepository NotificationMedia { get; }
        INotificationRecipientRepository NotificationRecipient { get; }
        ISystemAreaRepository SystemArea { get; }
        ISystemSubAreaRepository SystemSubArea { get; }

        void Save();
    }
}
