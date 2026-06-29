namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IAccountingAccountRepository AccountingAccounts{ get; }
        IAccountPayableRepository AccountPayable { get; }
        IAccountPayableMovementRepository AccountPayableMovement { get; }
        IBonuslyRepository Bonusly { get; }
        ICostCenterRepository CenterOfCosts { get; }
        ILedgerMovementRepository LedgerMovements { get; }
        IDataUpdateDateRepository DataUpdateDates { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IApplicationUserActiveHistoryRepository ApplicationUserActiveHistory { get; }
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
        ICompanyRepository Company { get; }
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
        IDocumentCCSubtypeRepository DocumentCCSubtype { get; }
        IDocumentTypeRepository DocumentType { get; }
        IInterviewRepository Interview { get; }
        IImageBlobRepository ImageBlob { get; }
        IJournalAccountPayableRepository JournalAccountPayable { get; }
        IJournalAccountPayableEntryRepository JournalAccountPayableEntry { get; }
        IPaymentMethodRepository PaymentMethod { get; }
        IPaymentBookEntryParentRepository PaymentBookEntryParent { get; }
        IProductRepository Product { get; }
        IProductClientCompanyAccountingConfigForBillingRepository ProductClientCompanyAccountingConfig { get; }
        IProjectRepository Project { get; }
        IProjectConsultantAssignedRepository ProjectConsultantAssigned { get; }
        IProjectConsultantPeriodDisabledTrackingRepository ProjectConsultantPeriodDisabledTracking { get; }
        IProjectConsultantAssignedHistoryRepository ProjectConsultantAssignedHistory { get; }
        IProjectConsultantPendingSubmissionRepository ProjectConsultantPendingSubmission { get; }
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
        ITeamRepository Team { get; }
        ICheckInRepository CheckIn { get; }
        IIssueRepository Issue { get; }
        ITransactionStatusRepository TransactionStatus { get; }
        ITimeOffRequestRepository TimeOffRequest { get; }

        Task SaveAsync();
        Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTranAsync();
    }
}
