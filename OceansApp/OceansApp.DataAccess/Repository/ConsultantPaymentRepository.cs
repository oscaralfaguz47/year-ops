using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantPayments;
using OceansApp.Models.ViewModels.ConsultantPaymentsDebitsCredits;
using OceansApp.Models.ViewModels.ConsultantReimbursedBenefits;
using OceansApp.Models.ViewModels.Consultants;
using OceansApp.Models.ViewModels.Interviews;
using OceansApp.Models.ViewModels.PaymentSheets;
using OceansApp.Models.ViewModels.ProjectConsultantAssigned;
using OceansApp.Models.ViewModels.ReportingMyTime;
using OceansApp.Utility.SharedMethods;
using System.Data;
using OceansApp.Models.ViewModels.ProjectConsultantAssignedHistory;
using OceansApp.Models.ViewModels.AccountsPayable;



namespace OceansApp.DataAccess.Repository
{
    public class ConsultantPaymentRepository : Repository<ConsultantPayment>, IConsultantPaymentRepository
    {
        private ApplicationDbContext _db;
        public ConsultantPaymentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<MethodResponse> GetMovementsToPay(ConsultantUserVM consultant, DateTime startDate,
            DateTime endDate)
        {
            if (consultant == null)
            {
                return new MethodResponse { MessageType = "Not Found", Success = false, Message = "Consultant not found." };
            }

            var connection = _db.Database.GetDbConnection();
            var sharedParameters = new DynamicParameters();
            sharedParameters.Add("@ConsultantId", consultant.ConsultantId);
            sharedParameters.Add("@StartDate", startDate);
            sharedParameters.Add("@EndDate", endDate);

            var activeProjects = await connection.QueryAsync<GetProjectInfoWhereConsultantIsActiveInProjectVM>("SP_PAYMENT_SHEETS_GetProjectsInfoWhereConsultantIsActiveInPeriod", sharedParameters, commandType: CommandType.StoredProcedure);

            var defaultProject = activeProjects.FirstOrDefault(p => p.IsDefaultProject == true);

            if (defaultProject == null)
            {
                return new MethodResponse { MessageType = "Not Found", Success = false, Message = "Default project not found." };
            }

            bool holidaysMustBePaid = defaultProject.IsDefaultProject && defaultProject.HolidaysMustBePaid ? true : false;
            decimal defaultHourlyCalculation = defaultProject.HourlySalary;

            if (defaultProject.MonthlySalary > 0 && defaultProject.IsMonthlySalaryCalculatedPerHour)
            {
                defaultHourlyCalculation = (defaultProject.MonthlySalary / DateAndTimes.GetWorkingDaysInMonth(startDate)) / 8;
            }
            GetListOfMovementsForPaymentVM reportToSend = new();

            List<GetPaymentDetailsMovementsVM> paymentProjectMovements = new();

            //Add movement for every project
            foreach (var project in activeProjects)
            {
                var projectMovementsParameters = new DynamicParameters();
                projectMovementsParameters.Add("@ConsultantId", consultant.ConsultantId);
                projectMovementsParameters.Add("@ProjectId", project.ProjectId);
                projectMovementsParameters.Add("@StartDate", startDate);
                projectMovementsParameters.Add("@EndDate", endDate);

                var projectMovements = await connection.QueryAsync<GetApprovedMovementsWhereConsultantVM>("SP_REPORTING_MY_TIME_MOVEMENTS_GetApprovedMovementsWhereConsultant", projectMovementsParameters, commandType: CommandType.StoredProcedure);
                if (project.MonthlySalary > 0 || project.HourlySalary > 0)
                {
                    if (project.AccessToTrackingTool)
                    {
                        foreach (var movement in projectMovements)
                        {
                            if (movement.MovementTypeName == "Normal Hours")
                            {
                                GetPaymentDetailsMovementsVM paymentProjectMovement = new()
                                {
                                    PaymentType = "Hours/normal payment",
                                    ProjectId = project.ProjectId,
                                    ProjectName = project.ProjectName,
                                    MovementTypeId = movement.MovementTypeId,
                                    MovementTypeName = project.IsMonthlySalaryCalculatedPerHour || project.HourlySalary > 0 ? "Hours of professional services" : "Professional services",
                                    Quantity = project.IsMonthlySalaryCalculatedPerHour || project.HourlySalary > 0 ? movement.TotalQuantity : 1,
                                    UnitPrice = project.HourlySalary > 0 ? project.HourlySalary : project.IsMonthlySalaryCalculatedPerHour ? (project.MonthlySalary / DateAndTimes.GetWorkingDaysInMonth(startDate)) / 8 : (consultant.PaymentPeriod == 1 ? (project.MonthlySalary / 2) : project.MonthlySalary)
                                };
                                paymentProjectMovements.Add(paymentProjectMovement);
                            }
                            else
                            {
                                GetPaymentDetailsMovementsVM paymentProjectMovement = new()
                                {
                                    PaymentType = "Hours/normal payment",
                                    ProjectId = project.ProjectId,
                                    ProjectName = project.ProjectName,
                                    MovementTypeId = movement.MovementTypeId,
                                    MovementTypeName = movement.MovementTypeName,
                                    Quantity = movement.TotalQuantity,
                                    UnitPrice = movement.MovementTypeName == "On Call Flate Rate" ? 500 : project.HourlySalary > 0 ? (project.HourlySalary * 2) : ((project.MonthlySalary / DateAndTimes.GetWorkingDaysInMonth(startDate)) / 8) * 2
                                };
                                paymentProjectMovements.Add(paymentProjectMovement);
                            }
                        }
                    }
                    else
                    {
                        var movementTypeNormalHours = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.FirstOrDefaultAsync(x => x.Name == "Normal Hours");

                        GetPaymentDetailsMovementsVM paymentProjectMovement = new()
                        {
                            PaymentType = "Hours/normal payment",
                            ProjectId = project.ProjectId,
                            ProjectName = project.ProjectName,
                            MovementTypeId = movementTypeNormalHours.MovementTypeId,
                            MovementTypeName = project.IsMonthlySalaryCalculatedPerHour || project.HourlySalary > 0 ? "Hours of professional services" : "Professional services",
                            Quantity = !project.IsMonthlySalaryCalculatedPerHour && project.MonthlySalary > 0 ? 1 : consultant.PaymentPeriod == 1 ? 80 : 160,
                            UnitPrice = project.HourlySalary > 0 ? project.HourlySalary :
                            project.IsMonthlySalaryCalculatedPerHour && project.MonthlySalary > 0 ? ((consultant.PaymentPeriod == 1 ? (project.MonthlySalary / 2) : project.MonthlySalary) / (consultant.PaymentPeriod == 1 ? 80 : 160)) : (consultant.PaymentPeriod == 1 ? (project.MonthlySalary / 2) : project.MonthlySalary)
                        };
                        paymentProjectMovements.Add(paymentProjectMovement);
                    }
                }
            }
            List<GetPaymentDetailsMovementsVM> benefitsAndOtherMovements = new();

            //Add Holidays
            if (holidaysMustBePaid)
            {
                var holidays = consultant.ConsultantHolidayId == null ? null : await _db.CONSULTANT_HOLIDAY_DATES
              .Where(x => x.ConsultantHolidayId == consultant.ConsultantHolidayId
                          && x.Date >= startDate
                          && x.Date <= endDate)
              .ToListAsync();

                if (holidays != null)
                {
                    var holidaysMovementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.FirstOrDefaultAsync(x => x.Name == "Holidays");
                    foreach (var holiday in holidays)
                    {
                        GetPaymentDetailsMovementsVM holidayMovement = new()
                        {
                            ProjectId = defaultProject.ProjectId,
                            PaymentType = "Holidays",
                            MovementTypeId = holidaysMovementType.MovementTypeId,
                            MovementTypeName = "Holiday - " + holiday.Name + " (" + holiday.Date.ToString("MM/dd/yyyy") + ")",
                            Quantity = 8,
                            UnitPrice = defaultHourlyCalculation
                        };
                        benefitsAndOtherMovements.Add(holidayMovement);
                    }
                }
            }

            //Add Benefits
            var benefits = await connection.QueryAsync<GetApprovedBenefitsWhereConsultant>("SP_CONSULTANT_REIMBURSED_BENEFITS_GetApprovedBenefitsWhereConsultantInThePeriod", sharedParameters, commandType: CommandType.StoredProcedure);

            foreach (var benefit in benefits)
            {
                GetPaymentDetailsMovementsVM benefitMovement = new()
                {
                    ProjectId = defaultProject.ProjectId,
                    PaymentType = "Reimbursed Benefits",
                    MovementTypeId = benefit.MovementTypeId,
                    MovementTypeName = benefit.MovementTypeName,
                    Quantity = 1,
                    UnitPrice = benefit.AmountReimbursed
                };
                benefitsAndOtherMovements.Add(benefitMovement);
            }
            //Add Interviews
            var interviews = await connection.QueryAsync<GetApprovedInterviewsWhereConsultantVM>("SP_INTERVIEWS_GetApprovedInterviewsWhereConsultantInThePeriod", sharedParameters, commandType: CommandType.StoredProcedure);

            foreach (var interview in interviews)
            {
                GetPaymentDetailsMovementsVM interviewMovement = new()
                {
                    ProjectId = defaultProject.ProjectId,
                    PaymentType = "Interviews",
                    MovementTypeId = interview.MovementTypeId,
                    MovementTypeName = interview.MovementTypeName,
                    Quantity = interview.TotalDurationHours,
                    UnitPrice = defaultHourlyCalculation
                };
                benefitsAndOtherMovements.Add(interviewMovement);
            }
            //Add Debits and Credits
            var debitsAndCredits = await connection.QueryAsync<GetApprovedDebitsCreditsWhereConsultantVM>("SP_CONSULTANT_PAYMENTS_DEBITS_CREDITS_GetApprovedDebitCreditWhereConsultantInThePeriod", sharedParameters, commandType: CommandType.StoredProcedure);

            List<GetPaymentDetailsMovementsVM> debitsMovements = new();

            foreach (var debitCredit in debitsAndCredits)
            {
                if (debitCredit.TransactionTypeName == "Credit")
                {
                    GetPaymentDetailsMovementsVM creditMovement = new()
                    {
                        ProjectId = defaultProject.ProjectId,
                        MovementId = debitCredit.ConsultantPaymentDebitsCreditsId,
                        PaymentType = "Credit",
                        MovementTypeName = debitCredit.Detail,
                        Quantity = debitCredit.Quantity,
                        UnitPrice = debitCredit.Amount
                    };
                    benefitsAndOtherMovements.Add(creditMovement);
                }
                else
                {
                    GetPaymentDetailsMovementsVM debitMovement = new()
                    {
                        ProjectId = defaultProject.ProjectId,
                        MovementId = debitCredit.ConsultantPaymentDebitsCreditsId,
                        PaymentType = "Debit",
                        MovementTypeName = debitCredit.Detail,
                        Quantity = debitCredit.Quantity,
                        UnitPrice = debitCredit.Amount
                    };
                    debitsMovements.Add(debitMovement);
                }
            }
            reportToSend.ProjectMovements = paymentProjectMovements;
            reportToSend.BenefitsAndOtherMovements = benefitsAndOtherMovements;
            reportToSend.DebitsMovements = debitsMovements;

            return new MethodResponse { Success = true, GenericList = reportToSend };
        }

        public async Task<MethodResponse> CreatePayment(string userIdCreatedBy,
            CreateUpdateConsultantPaymentVM paymentData, decimal accountPayableAmount, GetListOfMovementsForPaymentVM listOfMovementsForPayment)
        {
            if (paymentData == null)
            {
                return MethodResponse.CreateFailureExceptionResponse("Data cannot be null.");
            }
            var existingAccountPayable = await _db.ACCOUNTS_PAYABLE.FirstOrDefaultAsync(x => x.ConsultantId == paymentData.ConsultantId &&
            x.StartDatePeriod == DateTime.Parse(paymentData.StartDatePeriod) && x.EndDatePeriod == DateTime.Parse(paymentData.EndDatePeriod));

            List<JournalAccountPayableEntry> journalEntriesToCreate = new();

            if (existingAccountPayable == null)
            {
                journalEntriesToCreate = await GetJournalEntriesReadyToCreate(listOfMovementsForPayment,
            (int)paymentData.ConsultantId, paymentData.CompanyId, DateTime.Parse(paymentData.EndDatePeriod), accountPayableAmount);
            }

            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    //Create the account payable movement
                    if (existingAccountPayable == null)
                    {
                        existingAccountPayable = await CreateAccountPayable(userIdCreatedBy,
                        paymentData, accountPayableAmount, journalEntriesToCreate);
                    }

                    if (paymentData.PaymentAmount > Math.Round(existingAccountPayable.BalanceAmount, 2))
                    {
                        return MethodResponse.CreateFailureValidationResponse($"The amount to pay must be less than or equal to the account payable balance amount.");
                    }

                    var consultantPaymentToCreate = new ConsultantPayment
                    {
                        ConsultantId = (int)paymentData.ConsultantId,
                        StartDatePeriod = DateTime.Parse(paymentData.StartDatePeriod),
                        EndDatePeriod = DateTime.Parse(paymentData.EndDatePeriod),
                        ReferenceNumber = paymentData.ReferenceNumber,
                        PaymentMethodId = (int)paymentData.PaymentMethodId,
                        PaymentAmount = (decimal)paymentData.PaymentAmount,
                        CreationDate = DateTime.UtcNow,
                        UserCreatedBy = userIdCreatedBy,
                        CompanyId = paymentData.CompanyId,
                        BankAccountId = (int)paymentData.BankAccountId,
                        AccountingDate = DateTime.Parse(paymentData.AccountingDate),
                        AccountPayableId = existingAccountPayable.AccountPayableId
                    };
                    await _db.CONSULTANT_PAYMENTS.AddAsync(consultantPaymentToCreate);
                    existingAccountPayable.BalanceAmount -= (decimal)paymentData.PaymentAmount;
                    if (existingAccountPayable.BalanceAmount == 0)
                    {
                        var transactionStatuses = await _db.TRANSACTION_STATUSES.Where(x => x.Name == "Paid" || x.Name == "Rejected").ToListAsync();

                        existingAccountPayable.TransactionStatusId = transactionStatuses.FirstOrDefault(x => x.Name == "Paid").TransactionStatusId;

                       await UpdateMovementsStatuses(transactionStatuses, DateTime.Parse(paymentData.StartDatePeriod), DateTime.Parse(paymentData.EndDatePeriod),
                       (int)paymentData.ConsultantId, "Paid");
                    }
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return MethodResponse.CreateSuccessResponse("Payment reported successfully!");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                }
            }
        }
        public async Task UpdateMovementsStatuses(List<TransactionStatus> transactionStatuses, DateTime startDate, DateTime endDate,
            int consultantId, string newStatus)
        {
            int transactionStatusNew = transactionStatuses.FirstOrDefault(x => x.Name == newStatus).TransactionStatusId;
            int transactionStatusRejected = transactionStatuses.FirstOrDefault(x => x.Name == "Rejected").TransactionStatusId;

            //Change movements transaction status
            var benefits = await _db.CONSULTANT_REIMBURSED_BENEFITS.Where(x => x.ConsultantId == consultantId &&
            (x.DateToBeReimbursed >= startDate && x.DateToBeReimbursed <= endDate)
            && x.TransactionStatusId != transactionStatusRejected).ToListAsync();

            var interviews = await _db.INTERVIEWS.Where(x => x.ConsultantId == consultantId &&
            (x.Date >= startDate && x.Date <= endDate)
            && x.TransactionStatusId != transactionStatusRejected).ToListAsync();

            var debitsCredits = await _db.CONSULTANT_PAYMENTS_DEBITS_CREDITS.Where(x => x.ConsultantId == consultantId &&
            (x.ActionDateWithinFortnight >= startDate && x.ActionDateWithinFortnight <= endDate)
            && x.TransactionStatusId != transactionStatusRejected).ToListAsync();

            foreach (var benefit in benefits)
            {
                benefit.TransactionStatusId = transactionStatusNew;
            }
            foreach (var interview in interviews)
            {
                interview.TransactionStatusId = transactionStatusNew;
            }
            foreach (var debitCredit in debitsCredits)
            {
                debitCredit.TransactionStatusId = transactionStatusNew;
            }
            await _db.SaveChangesAsync();
        }

        private async Task<List<JournalAccountPayableEntry>> GetJournalEntriesReadyToCreate(GetListOfMovementsForPaymentVM listOfMovementsForPayment,
            int consultantId, string companyId, DateTime endDate, decimal accountPayableAmount)
        {
            List<JournalAccountPayableEntry> entriesListToReturn = new();

            //Create Journal Entries
            foreach (var projectMovement in listOfMovementsForPayment.ProjectMovements)
            {
                var connection = _db.Database.GetDbConnection();
                var projectHistoryParameters = new DynamicParameters();
                projectHistoryParameters.Add("@ConsultantId", consultantId);
                projectHistoryParameters.Add("@ProjectId", projectMovement.ProjectId);
                projectHistoryParameters.Add("@EndDate", endDate);
                var currentProjectHistory = await connection.QueryAsync<GetCurrentHistoryVM>("SP_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_GetCurrentHistory", projectHistoryParameters, commandType: CommandType.StoredProcedure);

                var accountingConfig = await _db.CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION.FirstOrDefaultAsync(x => x.MovementTypeId == projectMovement.MovementTypeId &&
                x.CompanyId == companyId && x.PositionId == currentProjectHistory.FirstOrDefault().PositionId);

                JournalAccountPayableEntry journalEntryToCreate = new()
                {
                    CostCenterId = accountingConfig.CostCenterId,
                    AccountingAccountId = accountingConfig.AccountingAccountId,
                    Reference = projectMovement.MovementTypeName,
                    Debit = projectMovement.TotalAmount,
                    Credit = 0
                };
                entriesListToReturn.Add(journalEntryToCreate);
            }
            foreach (var benefitAndCredit in listOfMovementsForPayment.BenefitsAndOtherMovements)
            {
                if (benefitAndCredit.MovementId > 0)
                {
                    var debitCreditMovement = await _db.CONSULTANT_PAYMENTS_DEBITS_CREDITS.FirstOrDefaultAsync(x => x.ConsultantPaymentDebitsCreditsId == benefitAndCredit.MovementId);

                    JournalAccountPayableEntry journalEntryToCreate = new()
                    {
                        CostCenterId = debitCreditMovement.CostCenterId,
                        AccountingAccountId = debitCreditMovement.AccountingAccountId,
                        Reference = debitCreditMovement.Detail,
                        Debit = benefitAndCredit.TotalAmount,
                        Credit = 0
                    };

                    entriesListToReturn.Add(journalEntryToCreate);
                }
                else
                {
                    var connection = _db.Database.GetDbConnection();
                    var projectHistoryParameters = new DynamicParameters();
                    projectHistoryParameters.Add("@ConsultantId", consultantId);
                    projectHistoryParameters.Add("@ProjectId", benefitAndCredit.ProjectId);
                    projectHistoryParameters.Add("@EndDate", endDate);
                    var currentProjectHistory = await connection.QueryAsync<GetCurrentHistoryVM>("SP_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_GetCurrentHistory", projectHistoryParameters, commandType: CommandType.StoredProcedure);
                    var accountingConfig = await _db.CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION.FirstOrDefaultAsync(x => x.MovementTypeId == benefitAndCredit.MovementTypeId &&
                    x.CompanyId == companyId && x.PositionId == currentProjectHistory.FirstOrDefault().PositionId);

                    JournalAccountPayableEntry journalEntryToCreate = new()
                    {
                        CostCenterId = accountingConfig.CostCenterId,
                        AccountingAccountId = accountingConfig.AccountingAccountId,
                        Reference = benefitAndCredit.MovementTypeName,
                        Debit = benefitAndCredit.TotalAmount,
                        Credit = 0
                    };
                    entriesListToReturn.Add(journalEntryToCreate);
                }
            }
            foreach (var debitMovement in listOfMovementsForPayment.DebitsMovements)
            {
                var debitCreditMovement = await _db.CONSULTANT_PAYMENTS_DEBITS_CREDITS.FirstOrDefaultAsync(x => x.ConsultantPaymentDebitsCreditsId == debitMovement.MovementId);

                JournalAccountPayableEntry journalEntryToCreate = new()
                {
                    CostCenterId = debitCreditMovement.CostCenterId,
                    AccountingAccountId = debitCreditMovement.AccountingAccountId,
                    Reference = debitCreditMovement.Detail,
                    Debit = 0,
                    Credit = debitMovement.TotalAmount
                };

                entriesListToReturn.Add(journalEntryToCreate);
            }
            //Accounts payable entry
            var costCenter = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == "10-01-08" && x.CompanyId == companyId);
            var accountingAccount = await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountCode.Contains("2-01-01-002-000")
            && x.CompanyId == companyId);
            JournalAccountPayableEntry journalEntryAccountsPayableToCreate = new()
            {
                CostCenterId = costCenter.CostCenterId,
                AccountingAccountId = accountingAccount.AccountingAccountId,
                Reference = "Cuenta por pagar a consultor",
                Debit = 0,
                Credit = accountPayableAmount
            };
            entriesListToReturn.Add(journalEntryAccountsPayableToCreate);

            return entriesListToReturn;
        }

        public async Task<MethodResponse> SetAsAccountPayable(string userIdCreatedBy,
            SetAsAccountPayableVM dataFromModel, decimal accountPayableAmount, GetListOfMovementsForPaymentVM listOfMovementsForPayment,
            string companyId)
        {
            if (dataFromModel == null)
            {
                return MethodResponse.CreateFailureExceptionResponse("Data cannot be null.");
            }
            var existingAccountPayable = await _db.ACCOUNTS_PAYABLE.FirstOrDefaultAsync(x => x.ConsultantId == dataFromModel.ConsultantId &&
            x.StartDatePeriod == DateTime.Parse(dataFromModel.StartDatePeriod) && x.EndDatePeriod == DateTime.Parse(dataFromModel.EndDatePeriod));

            if (existingAccountPayable != null)
            {
                return MethodResponse.CreateFailureValidationResponse($"There is already an account payable for this consultant in the period.");
            }

            List<JournalAccountPayableEntry> journalEntriesToCreate = new();

            journalEntriesToCreate = await GetJournalEntriesReadyToCreate(listOfMovementsForPayment,
            (int)dataFromModel.ConsultantId, companyId, DateTime.Parse(dataFromModel.EndDatePeriod), accountPayableAmount);

            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    //Create the account payable movement
                    CreateUpdateConsultantPaymentVM completeModel = new()
                    {
                        ConsultantId = dataFromModel.ConsultantId,
                        CompanyId = companyId,
                        StartDatePeriod = dataFromModel.StartDatePeriod,
                        EndDatePeriod = dataFromModel.EndDatePeriod
                    };

                    existingAccountPayable = await CreateAccountPayable(userIdCreatedBy,
                    completeModel, accountPayableAmount, journalEntriesToCreate);

                    await transaction.CommitAsync();

                    return MethodResponse.CreateSuccessResponse("Reported as account payable successfully!");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                }
            }
        }

        private async Task<AccountPayable> CreateAccountPayable(string userIdCreatedBy,
            CreateUpdateConsultantPaymentVM paymentData, decimal accountPayableAmount, List<JournalAccountPayableEntry> journalEntriesToCreate)
        {
            //Create the account payable movement

            var transactionStatuses = await _db.TRANSACTION_STATUSES.Where(x => x.Name == "Sent to be paid" || 
            x.Name == "Pending to register" || x.Name == "Rejected").ToListAsync();

            AccountPayable accountPayableToCreate = new()
            {
                ConsultantId = (int)paymentData.ConsultantId,
                StartDatePeriod = DateTime.Parse(paymentData.StartDatePeriod),
                EndDatePeriod = DateTime.Parse(paymentData.EndDatePeriod),
                AccountingDate = DateTime.Parse(paymentData.EndDatePeriod),
                Amount = accountPayableAmount,
                BalanceAmount = accountPayableAmount,
                CreationDate = DateTime.UtcNow,
                UserCreatedBy = userIdCreatedBy,
                CompanyId = paymentData.CompanyId,
                TransactionStatusId = transactionStatuses.FirstOrDefault(x => x.Name == "Sent to be paid").TransactionStatusId
            };

            await _db.ACCOUNTS_PAYABLE.AddAsync(accountPayableToCreate);
            await _db.SaveChangesAsync();

            //Create the Journal and Journal Entries
            var existingJournal = await _db.JOURNAL_ACCOUNTS_PAYABLE.FirstOrDefaultAsync(x => x.StartDatePeriod == DateTime.Parse(paymentData.StartDatePeriod) &&
                x.EndDatePeriod == DateTime.Parse(paymentData.EndDatePeriod) && x.CompanyId == paymentData.CompanyId);
            if (existingJournal == null)
            {
                var journalConsecutive = await _db.GLOBAL_CONSECUTIVES.FirstOrDefaultAsync(x => x.Name == "JOURNAL_CXP" && x.CompanyId == paymentData.CompanyId);

                journalConsecutive.ConsecutiveNumber++;
                JournalAccountPayable journalToCreate = new()
                {
                    CompanyId = paymentData.CompanyId,
                    TransactionStatusId = transactionStatuses.FirstOrDefault(x => x.Name == "Pending to register").TransactionStatusId,
                    StartDatePeriod = DateTime.Parse(paymentData.StartDatePeriod),
                    EndDatePeriod = DateTime.Parse(paymentData.EndDatePeriod),
                    Entry = $"OCXPF{journalConsecutive.ConsecutiveNumber.ToString().PadLeft(5, '0')}",
                    AccountingPackage = "OCXP",
                    EntryType = "OCXP",
                    AccountingDate = DateTime.Parse(paymentData.EndDatePeriod),
                    CreationDate = DateTime.UtcNow,
                    UserCreatedBy = userIdCreatedBy
                };
                await _db.JOURNAL_ACCOUNTS_PAYABLE.AddAsync(journalToCreate);
                await _db.SaveChangesAsync();
                existingJournal = journalToCreate;
            }

            //Create Journal Entries
            foreach (var journalEntry in journalEntriesToCreate)
            {
                journalEntry.AccountPayableId = accountPayableToCreate.AccountPayableId;
                journalEntry.JournalId = existingJournal.JournalId;

                await _db.JOURNAL_ACCOUNTS_PAYABLE_ENTRIES.AddAsync(journalEntry);
                await _db.SaveChangesAsync();
            }
            //Change movements transaction status
            await UpdateMovementsStatuses(transactionStatuses, DateTime.Parse(paymentData.StartDatePeriod), DateTime.Parse(paymentData.EndDatePeriod),
                       (int)paymentData.ConsultantId, "Sent to be paid");

            return accountPayableToCreate;
        }

        public async Task<MethodResponse> UpdatePayment(string userIdCreatedBy,
            CreateUpdateConsultantPaymentVM paymentData)
        {
            if (paymentData == null)
            {
                return MethodResponse.CreateFailureExceptionResponse("Data cannot be null.");
            }
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingPayment = await _db.CONSULTANT_PAYMENTS.FirstOrDefaultAsync(x => x.ConsultantPaymentId == paymentData.ConsultantPaymentId);
                    if (existingPayment == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The payment no longer exists.");
                    }
                    var existingAccountPayable = await _db.ACCOUNTS_PAYABLE.FirstOrDefaultAsync(x => x.ConsultantId == paymentData.ConsultantId &&
                    x.StartDatePeriod == DateTime.Parse(paymentData.StartDatePeriod) && x.EndDatePeriod == DateTime.Parse(paymentData.EndDatePeriod));

                    if (existingAccountPayable == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Account payable does not exist.");
                    }
                    decimal accountPayableDefaultBalance = existingAccountPayable.BalanceAmount;

                    if (paymentData.PaymentAmount > (existingAccountPayable.BalanceAmount + (decimal)existingPayment.PaymentAmount))
                    {
                        return MethodResponse.CreateFailureValidationResponse($"The amount to pay must be less than or equal to the account payable balance amount.");
                    }

                    existingAccountPayable.BalanceAmount = (existingPayment.PaymentAmount + (decimal)existingAccountPayable.BalanceAmount) - (decimal)paymentData.PaymentAmount;

                    var transactionStatuses = await _db.TRANSACTION_STATUSES.Where(x => x.Name == "Paid" || x.Name == "Sent to be paid" || x.Name == "Rejected").ToListAsync();
                    int transactionStatusPaid = transactionStatuses.FirstOrDefault(x => x.Name == "Paid").TransactionStatusId;

                    if (existingAccountPayable.BalanceAmount == 0)
                    {
                        existingAccountPayable.TransactionStatusId = transactionStatusPaid;

                        await UpdateMovementsStatuses(transactionStatuses, DateTime.Parse(paymentData.StartDatePeriod), DateTime.Parse(paymentData.EndDatePeriod),
                       (int)paymentData.ConsultantId, "Paid");
                    }
                    if (accountPayableDefaultBalance == 0)
                    {
                        await UpdateMovementsStatuses(transactionStatuses, DateTime.Parse(paymentData.StartDatePeriod), DateTime.Parse(paymentData.EndDatePeriod),
                       (int)paymentData.ConsultantId, "Sent to be paid");
                    }
                    existingPayment.ReferenceNumber = paymentData.ReferenceNumber;
                    existingPayment.PaymentMethodId = (int)paymentData.PaymentMethodId;
                    existingPayment.PaymentAmount = (int)paymentData.PaymentAmount;
                    existingPayment.CompanyId = paymentData.CompanyId;
                    existingPayment.BankAccountId = (int)paymentData.BankAccountId;
                    existingPayment.AccountingDate = DateTime.Parse(paymentData.AccountingDate);
                    existingPayment.UserLastUpdatedBy = userIdCreatedBy;
                    existingPayment.LastUpdatedDate = DateTime.UtcNow;

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return MethodResponse.CreateSuccessResponse("Payment updated successfully!");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                }
            }
        }

        public async Task<List<GetConsultantPaymentsInPeriodVM>> GetConsultantPaymentsInPeriod(int consultantId, DateTime startDate,
            DateTime endDate)
        {
            var result = await (from cp in _db.CONSULTANT_PAYMENTS
                                join pm in _db.PAYMENT_METHODS on cp.PaymentMethodId equals pm.PaymentMethodId
                                join ba in _db.BANK_ACCOUNTS on cp.BankAccountId equals ba.BankAccountId
                                where cp.ConsultantId == consultantId && (cp.StartDatePeriod >= startDate && cp.EndDatePeriod <= endDate)
                                select new GetConsultantPaymentsInPeriodVM
                                {
                                    ConsultantPaymentId = cp.ConsultantPaymentId,
                                    ReferenceNumber = cp.ReferenceNumber,
                                    AccountingDate = cp.AccountingDate,
                                    CompanyId = cp.CompanyId,
                                    PaymentAmount = cp.PaymentAmount,
                                    PaymentMethodName = pm.Name,
                                    BankAccountName = ba.BankAccountName
                                }).ToListAsync();
            return result;
        }

    }
}
