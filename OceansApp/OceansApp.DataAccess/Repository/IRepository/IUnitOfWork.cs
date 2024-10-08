namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IAccountingAccountRepository AccountingAccounts{ get; }
        IAccountPayableRepository AccountPayable { get; }
        IAccountPayableMovementRepository AccountPayableMovement { get; }
        ICostCenterRepository CenterOfCosts { get; }
        ILedgerMovementRepository LedgerMovements { get; }
        IDataUpdateDateRepository DataUpdateDates { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IApplicationUserCategoryRepository ApplicationUserCategory { get; }
        IApplicationRoleClaimRepository ApplicationRoleClaim { get; }
        IApplicationSystemClaimRepository ApplicationSystemClaim { get; }
        IBankAccountRepository BankAccount { get; }
        ICalculatorGlobalConfigurationRepository CalculatorGlobalConfiguration { get; }
        ICalculatorCostCenterIncreaseConfigurationRepository CalculatorCostCenterIncreaseConfiguration { get; }
        ICalculatorSearchHistoryRepository CalculatorSearchHistory { get; }
        ICalculatorAccountingAccountToIgnoreRepository CalculatorAccountingAccountToIgnore { get; }
        IClientRepository Client { get; }
        IPartnerRepository Partner { get; }
        ICountryRepository Country { get; }
        IConsultantBenefitRepository ConsultantBenefit { get; }
        IConsultantAndBenefitRepository ConsultantAndBenefit { get; }
        IConsultantBenefitCategoryRepository ConsultantBenefitCategory { get; }
        IConsultantReimbursedBenefitRepository ConsultantReimbursedBenefit { get; }
        IConsultantDetailRepository ConsultantDetail { get; }
        IConsultantHolidayRepository ConsultantHoliday { get; }
        IConsultantPaymentRepository ConsultantPayment { get; }
        IConsultantPaymentDebitsCreditsRepository ConsultantPaymentsDebitsCredits { get; }
        IConsultantPositionRepository ConsultantPosition { get; }
        IConsultantRoleRepository ConsultantRole { get; }
        IConsultantQualityLevelRepository ConsultantQualityLevel { get; }
        IConsultantRoleQualityLevelRepository ConsultantRoleQualityLevel { get; }
        IConsultantSeniorityRepository ConsultantSeniority { get; }
        ICostCenterAccountingAccountRepository CostCenterAccountingAccount { get; }
        IInterviewRepository Interview { get; }
        IJournalAccountPayableRepository JournalAccountPayable { get; }
        IJournalAccountPayableEntryRepository JournalAccountPayableEntry { get; }
        IPaymentMethodRepository PaymentMethod { get; }
        IPaymentBookEntryParentRepository PaymentBookEntryParent { get; }
        IProjectRepository Project { get; }
        IProjectConsultantAssignedRepository ProjectConsultantAssigned { get; }
        IProjectConsultantPeriodDisabledTrackingRepository ProjectConsultantPeriodDisabledTracking { get; }
        IProjectConsultantAssignedHistoryRepository ProjectConsultantAssignedHistory { get; }
        IProjectUserSelectedRepository ProjectUserSelected { get; }
        IReportingMyTimeMovementRepository ReportingMyTimeMovement { get; }
        IReportingMyTimeMovementSubmissionRepository ReportingMyTimeMovementSubmission { get; }
        IReportingMyTimeMovementTypeRepository ReportingMyTimeMovementType { get; }
        IDocumentCCRepository DocumentCC { get; }
        IDocumentsCCNotificationRepository DocumentsCCNotification { get; }
        INotificationTypeRepository NotificationType { get; }
        INotificationRepository Notification { get; }
        INotificationStatusRepository NotificationStatus { get; }
        INotificationMediaRepository NotificationMedia { get; }
        INotificationRecipientRepository NotificationRecipient { get; }
        ISystemAreaRepository SystemArea { get; }
        ISystemSubAreaRepository SystemSubArea { get; }
        ITransactionStatusRepository TransactionStatus { get; }

        Task SaveAsync();
        Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTranAsync();
    }
}
