using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantPayments;
using OceansApp.Models.ViewModels.ConsultantPaymentsDebitsCredits;
using OceansApp.Models.ViewModels.ConsultantReimbursedBenefits;
using OceansApp.Models.ViewModels.Interviews;
using OceansApp.Models.ViewModels.PaymentSheets;
using OceansApp.Models.ViewModels.ProjectConsultantAssigned;
using OceansApp.Models.ViewModels.ReportingMyTime;
using OceansApp.Utility.SharedMethods;
using System.Data;
using OceansApp.Models.ViewModels.ProjectConsultantAssignedHistory;
using OceansApp.Models.ViewModels.AccountsPayable;
using Microsoft.Data.SqlClient;
using OceansApp.Models.ViewModels.Consultants;
using OceansApp.Models.ViewModels;
using OceansApp.Utility.NotificationTemplates;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Queues;
using Newtonsoft.Json;



namespace OceansApp.DataAccess.Repository
{
    public class ConsultantPaymentRepository : Repository<ConsultantPayment>, IConsultantPaymentRepository
    {
        private ApplicationDbContext _db;
        private readonly IConsultantDetailRepository _consultantDetailRepository;
        private readonly IProjectConsultantAssignedHistoryRepository _projectConsultantAssignedHistoryRepository;
        private readonly IConfiguration _config;
        private readonly Lazy<QueueClient> _queueClient;
        public ConsultantPaymentRepository(ApplicationDbContext db, IUnitOfWork unitOfWork, IConfiguration config,
            Lazy<QueueClient> queueClient) : base(db)
        {
            _db = db;
            _consultantDetailRepository = unitOfWork.ConsultantDetail;
            _projectConsultantAssignedHistoryRepository = unitOfWork.ProjectConsultantAssignedHistory;
            _config = config;
            _queueClient = queueClient;
        }
        public async Task<MethodResponse> GetMovementsToPay(ConsultantUserVM consultant, DateTime startDate,
            DateTime endDate)
        {
            if (consultant == null)
            {
                return new MethodResponse { MessageType = "Not Found", Success = false, Message = "Consultant not found." };
            }

            var existingAccountsPayableList = await _db.ACCOUNTS_PAYABLE.Where(x => x.Voided == false && x.ConsultantId == consultant.ConsultantId && (x.StartDatePeriod >= startDate &&
            x.EndDatePeriod <= endDate)).Include(x => x.TransactionStatus).ToListAsync();

            var closestToEndDate = existingAccountsPayableList
    .OrderBy(x => Math.Abs((x.EndDatePeriod - endDate).TotalDays))
    .FirstOrDefault();

            GetListOfMovementsForPaymentVM reportToSend = new();

            if (existingAccountsPayableList.Count > 0 &&
    (closestToEndDate != null &&
    (closestToEndDate.TransactionStatus.Name == "Paid" ||
     closestToEndDate.TransactionStatus.Name == "Sent to be paid")))
            {
                for (int i = 0; i < existingAccountsPayableList.Count; i++)
                {
                    var accountPayable = existingAccountsPayableList[i];
                    var movementsList = await GetPaidMovementsAsync(accountPayable.AccountPayableId);

                    if (movementsList.ProjectMovements != null)
                    {
                        reportToSend.ProjectMovements = new List<GetPaymentDetailsMovementsVM>();
                        foreach (var movement in movementsList.ProjectMovements)
                        {
                            reportToSend.ProjectMovements.Add(movement);
                        }
                    }
                    if (movementsList.BenefitsAndOtherMovements != null)
                    {
                        reportToSend.BenefitsAndOtherMovements = new List<GetPaymentDetailsMovementsVM>();
                        foreach (var movement in movementsList.BenefitsAndOtherMovements)
                        {
                            reportToSend.BenefitsAndOtherMovements.Add(movement);
                        }
                    }
                    if (movementsList.DebitsMovements != null)
                    {
                        reportToSend.DebitsMovements = new List<GetPaymentDetailsMovementsVM>();
                        foreach (var movement in movementsList.DebitsMovements)
                        {
                            reportToSend.DebitsMovements.Add(movement);
                        }
                    }
                }
            }
            else
            {
                var connection = _db.Database.GetDbConnection();
                var sharedParameters = new DynamicParameters();
                sharedParameters.Add("@ConsultantId", consultant.ConsultantId);
                sharedParameters.Add("@StartDate", startDate);
                sharedParameters.Add("@EndDate", endDate);

                var activeProjects = await connection.QueryAsync<GetProjectInfoWhereConsultantIsActiveInProjectVM>("SP_PAYMENT_SHEETS_GetProjectsInfoWhereConsultantIsActiveInPeriod", sharedParameters, commandType: CommandType.StoredProcedure);

                var defaultProject = activeProjects.FirstOrDefault(p => p.IsDefaultProject == true);

                if (defaultProject == null)
                {
                    defaultProject = activeProjects.FirstOrDefault();
                    if (defaultProject == null)
                    {
                        return new MethodResponse { MessageType = "Not Found", Success = false, Message = "Default project not found." };
                    }
                }

                bool holidaysMustBePaid = defaultProject.IsDefaultProject && defaultProject.HolidaysMustBePaid ? true : false;
                decimal defaultHourlyCalculation = defaultProject.MonthlySalaryPartner > 0 ? ((defaultProject.MonthlySalaryPartner / DateAndTimes.GetWorkingDaysInMonth(startDate)) / 8) + defaultProject.HourlySalary : defaultProject.HourlySalary;

                if (defaultProject.MonthlySalary > 0)
                {
                    defaultHourlyCalculation = defaultProject.MonthlySalaryPartner > 0 ? ((defaultProject.MonthlySalaryPartner / DateAndTimes.GetWorkingDaysInMonth(startDate)) / 8) + (defaultProject.MonthlySalary / DateAndTimes.GetWorkingDaysInMonth(startDate)) / 8 : (defaultProject.MonthlySalary / DateAndTimes.GetWorkingDaysInMonth(startDate)) / 8;
                }

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
                        if (project.AccessToTrackingTool && project.IsMonthlySalaryCalculatedPerHour)
                        {
                            foreach (var movement in projectMovements)
                            {
                                if (movement.MovementTypeName == "Normal Hours")
                                {
                                    GetPaymentDetailsMovementsVM paymentProjectMovement = new()
                                    {
                                        MovementId = movement.MovementId,
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
                                        MovementId = movement.MovementId,
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
                                Quantity = 1,
                                UnitPrice = consultant.PaymentPeriod == 1 ? (project.MonthlySalary / 2) : project.MonthlySalary
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
                        MovementId = benefit.MovementId,
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
                        MovementId = interview.MovementId,
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
            }
            return new MethodResponse { Success = true, GenericList = reportToSend };
        }

        private async Task<GetListOfMovementsForPaymentVM> GetPaidMovementsAsync(int accountPayableId)
        {
            try
            {
                GetListOfMovementsForPaymentVM dataToReturn = new();

                var projectMovements = await (from apm in _db.ACCOUNTS_PAYABLE_MOVEMENTS
                                              join p in _db.PROJECTS on apm.ProjectId equals p.ProjectId into projectGroup
                                              from p in projectGroup.DefaultIfEmpty() // Left join with Projects
                                              where apm.AccountPayableId == accountPayableId
                                              && apm.Type == "Hours/normal payment"
                                              select new GetPaymentDetailsMovementsVM
                                              {
                                                  MovementTypeName = apm.Description,
                                                  ProjectName = p.Name,
                                                  Quantity = apm.Quantity,
                                                  UnitPrice = apm.UnitPrice
                                              }).ToListAsync();

                dataToReturn.ProjectMovements = projectMovements;

                var benefitsAndOtherMovements = await (from apm in _db.ACCOUNTS_PAYABLE_MOVEMENTS
                                                       where apm.AccountPayableId == accountPayableId
                                                       && apm.Type != "Hours/normal payment" && apm.Type != "Debit"
                                                       select new GetPaymentDetailsMovementsVM
                                                       {
                                                           MovementTypeName = apm.Description,
                                                           Quantity = apm.Quantity,
                                                           UnitPrice = apm.UnitPrice
                                                       }).ToListAsync();

                dataToReturn.BenefitsAndOtherMovements = benefitsAndOtherMovements;

                var debitsMovements = await (from apm in _db.ACCOUNTS_PAYABLE_MOVEMENTS
                                             where apm.AccountPayableId == accountPayableId
                                             && apm.Type == "Debit"
                                             select new GetPaymentDetailsMovementsVM
                                             {
                                                 MovementTypeName = apm.Description,
                                                 Quantity = apm.Quantity,
                                                 UnitPrice = apm.UnitPrice
                                             }).ToListAsync();

                dataToReturn.DebitsMovements = debitsMovements;

                return dataToReturn;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<MethodResponse> CreatePayment(string userIdCreatedBy,
    CreateUpdateConsultantPaymentVM paymentData, decimal accountPayableAmount, GetListOfMovementsForPaymentVM listOfMovementsForPayment)
        {
            // Validate if payment data is null
            if (paymentData == null) return MethodResponse.CreateFailureExceptionResponse("Data cannot be null.");

            // Retrieve the existing account payable by consultant and period
            var existingAccountPayable = await _db.ACCOUNTS_PAYABLE.FirstOrDefaultAsync(x => x.ConsultantId == paymentData.ConsultantId &&
                x.StartDatePeriod == DateTime.Parse(paymentData.StartDatePeriod) && x.EndDatePeriod == DateTime.Parse(paymentData.EndDatePeriod)
                && x.Voided == false);

            // Prepare journal entries if no account payable exists
            List<JournalAccountPayableEntry> journalEntriesToCreate = new();
            if (existingAccountPayable == null)
            {
                journalEntriesToCreate = await GetJournalEntriesReadyToCreate(listOfMovementsForPayment,
                    (int)paymentData.ConsultantId, paymentData.CompanyId, DateTime.Parse(paymentData.EndDatePeriod), accountPayableAmount);
            }

            // Start database transaction
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Create account payable if it doesn't exist
                if (existingAccountPayable == null)
                {
                    existingAccountPayable = await CreateAccountPayable(userIdCreatedBy,
                        paymentData, accountPayableAmount, journalEntriesToCreate, listOfMovementsForPayment);
                }

                // Validate if the payment amount exceeds the account payable balance
                if (paymentData.PaymentAmount > Math.Round(existingAccountPayable.BalanceAmount, 2))
                    return MethodResponse.CreateFailureValidationResponse("The amount to pay must be less than or equal to the account payable balance.");

                // Create the consultant payment entry
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

                // Add the new payment entry and update the balance amount
                await _db.CONSULTANT_PAYMENTS.AddAsync(consultantPaymentToCreate);
                existingAccountPayable.BalanceAmount -= (decimal)paymentData.PaymentAmount;

                // Update transaction status if the balance becomes zero
                var transactionStatuses = await _db.TRANSACTION_STATUSES.Where(x => x.Name == "Paid" || x.Name == "Rejected" || x.Name == "Sent to be paid").ToListAsync();
                if (existingAccountPayable.BalanceAmount <= 0.0030m)
                {
                    existingAccountPayable.TransactionStatusId = transactionStatuses.FirstOrDefault(x => x.Name == "Paid").TransactionStatusId;

                    await UpdateMovementsStatuses(transactionStatuses, DateTime.Parse(paymentData.StartDatePeriod), DateTime.Parse(paymentData.EndDatePeriod),
                        (int)paymentData.ConsultantId, "Paid");
                }
                else
                {
                    existingAccountPayable.TransactionStatusId = transactionStatuses.FirstOrDefault(x => x.Name == "Sent to be paid").TransactionStatusId;
                }

                await _db.SaveChangesAsync();

                // Handle book entries: Retrieve or create a parent entry
                var transactionStatusesPending = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Pending Accounting");
                var existingBookEntryParent = await _db.PAYMENT_BOOK_ENTRIES_PARENT.FirstOrDefaultAsync(x => x.TransactionStatusId == transactionStatusesPending.TransactionStatusId
                    && x.CompanyId == paymentData.CompanyId);
                if (existingBookEntryParent == null)
                {
                    existingBookEntryParent = new PaymentBookEntryParent
                    {
                        TransactionStatusId = transactionStatusesPending.TransactionStatusId,
                        CompanyId = paymentData.CompanyId,
                        CreationDate = DateTime.UtcNow,
                        UserCreatedBy = userIdCreatedBy
                    };
                    await _db.PAYMENT_BOOK_ENTRIES_PARENT.AddAsync(existingBookEntryParent);
                    await _db.SaveChangesAsync();
                }

                // Create child book entry linked to the payment and parent
                var consultantToPay = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.ConsultantId == paymentData.ConsultantId);
                if (consultantToPay == null) return MethodResponse.CreateFailureNotFoundResponse("The consultant was not found.");

                var userToPay = await _db.AspNetUsers.FirstOrDefaultAsync(x => x.Id == consultantToPay.UserId);
                if (userToPay == null) return MethodResponse.CreateFailureNotFoundResponse("The consultant was not found.");

                var bookEntryChildToCreate = new PaymentBookEntryChild
                {
                    ParentId = existingBookEntryParent.ParentId,
                    ConsultantPaymentId = consultantPaymentToCreate.ConsultantPaymentId,
                    Notes = $"Payment to: {userToPay.Name} {userToPay.LastName}",
                    Voided = false
                };
                await _db.PAYMENT_BOOK_ENTRIES_CHILD.AddAsync(bookEntryChildToCreate);
                await _db.SaveChangesAsync();

                // Commit the transaction
                await transaction.CommitAsync();
                return MethodResponse.CreateSuccessResponse("Payment reported successfully!");
            }
            catch (DbUpdateException ex)
            {
                // Rollback the transaction in case of an error
                await transaction.RollbackAsync();

                // Check if the exception is caused by a unique constraint violation
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // SqlException error code 2601 is for duplicate keys
                {
                    return MethodResponse.CreateFailureValidationResponse("The reference number already exists. Please use a different one.");
                }
                else
                {
                    // Handle other exceptions
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                }
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of an error
                await transaction.RollbackAsync();
                return MethodResponse.CreateFailureExceptionResponse(ex.Message);
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

            // Loop through project movements to create journal entries
            foreach (var projectMovement in listOfMovementsForPayment.ProjectMovements)
            {
                var connection = _db.Database.GetDbConnection();
                var projectHistoryParameters = new DynamicParameters();
                projectHistoryParameters.Add("@ConsultantId", consultantId);
                projectHistoryParameters.Add("@ProjectId", projectMovement.ProjectId);
                projectHistoryParameters.Add("@EndDate", endDate);

                // Execute stored procedure to get current project history
                var currentProjectHistory = await connection.QueryAsync<GetCurrentHistoryVM>("SP_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_GetCurrentHistory",
                    projectHistoryParameters, commandType: CommandType.StoredProcedure);

                // Get the accounting configuration based on the movement type and position
                var accountingConfig = await _db.CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION
                    .FirstOrDefaultAsync(x => x.MovementTypeId == projectMovement.MovementTypeId && x.CompanyId == companyId &&
                    x.PositionId == currentProjectHistory.FirstOrDefault().PositionId);

                // Create journal entry for the project movement
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

            // Loop through benefits and other movements to create journal entries
            foreach (var benefitAndCredit in listOfMovementsForPayment.BenefitsAndOtherMovements)
            {
                if (benefitAndCredit.PaymentType == "Debit" || benefitAndCredit.PaymentType == "Credit")
                {
                    var debitCreditMovement = await _db.CONSULTANT_PAYMENTS_DEBITS_CREDITS
                        .FirstOrDefaultAsync(x => x.ConsultantPaymentDebitsCreditsId == benefitAndCredit.MovementId);

                    // Create journal entry for debit/credit movement
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
                    // Execute stored procedure to get project history if movement does not have an ID
                    var connection = _db.Database.GetDbConnection();
                    var projectHistoryParameters = new DynamicParameters();
                    projectHistoryParameters.Add("@ConsultantId", consultantId);
                    projectHistoryParameters.Add("@ProjectId", benefitAndCredit.ProjectId);
                    projectHistoryParameters.Add("@EndDate", endDate);

                    var currentProjectHistory = await connection.QueryAsync<GetCurrentHistoryVM>("SP_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_GetCurrentHistory",
                        projectHistoryParameters, commandType: CommandType.StoredProcedure);

                    // Get accounting configuration and create journal entry for the benefit/credit movement
                    var accountingConfig = await _db.CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION
                        .FirstOrDefaultAsync(x => x.MovementTypeId == benefitAndCredit.MovementTypeId && x.CompanyId == companyId &&
                        x.PositionId == currentProjectHistory.FirstOrDefault().PositionId);

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

            // Loop through debit movements to create journal entries
            foreach (var debitMovement in listOfMovementsForPayment.DebitsMovements)
            {
                var debitCreditMovement = await _db.CONSULTANT_PAYMENTS_DEBITS_CREDITS
                    .FirstOrDefaultAsync(x => x.ConsultantPaymentDebitsCreditsId == debitMovement.MovementId);

                // Create journal entry for debit movement
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

            // Create journal entry for accounts payable
            var costCenter = await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterCode == "10-01-08" && x.CompanyId == companyId);
            var accountingAccount = await _db.ACCOUNTING_ACCOUNT
                .FirstOrDefaultAsync(x => x.AccountingAccountCode.Contains("2-01-01-002-000") && x.CompanyId == companyId);

            JournalAccountPayableEntry journalEntryAccountsPayableToCreate = new()
            {
                CostCenterId = costCenter.CostCenterId,
                AccountingAccountId = accountingAccount.AccountingAccountId,
                Reference = "Cuenta por pagar a consultor",
                Debit = 0,
                Credit = Math.Round(accountPayableAmount, 2)
            };
            entriesListToReturn.Add(journalEntryAccountsPayableToCreate);

            // Return the list of all created journal entries
            return entriesListToReturn;
        }

        public async Task<MethodResponse> SetAsAccountPayable(string userIdCreatedBy,
    SetAsAccountPayableVM dataFromModel, decimal accountPayableAmount, GetListOfMovementsForPaymentVM listOfMovementsForPayment,
    string companyId)
        {
            // Validate if dataFromModel is null
            if (dataFromModel == null) return MethodResponse.CreateFailureExceptionResponse("Data cannot be null.");

            // Check if an account payable for this consultant and period already exists
            var existingAccountPayable = await _db.ACCOUNTS_PAYABLE.FirstOrDefaultAsync(x => x.ConsultantId == dataFromModel.ConsultantId &&
                x.StartDatePeriod == DateTime.Parse(dataFromModel.StartDatePeriod) && x.EndDatePeriod == DateTime.Parse(dataFromModel.EndDatePeriod)
                && x.Voided == false);

            // Return a validation failure if an account payable already exists
            if (existingAccountPayable != null)
                return MethodResponse.CreateFailureValidationResponse("There is already an account payable for this consultant in the period.");

            // Prepare journal entries for the new account payable
            var journalEntriesToCreate = await GetJournalEntriesReadyToCreate(listOfMovementsForPayment,
                (int)dataFromModel.ConsultantId, companyId, DateTime.Parse(dataFromModel.EndDatePeriod), accountPayableAmount);

            // Start a database transaction
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Create a complete model for account payable creation
                var completeModel = new CreateUpdateConsultantPaymentVM
                {
                    ConsultantId = dataFromModel.ConsultantId,
                    CompanyId = companyId,
                    StartDatePeriod = dataFromModel.StartDatePeriod,
                    EndDatePeriod = dataFromModel.EndDatePeriod
                };

                // Create the new account payable with the provided data and journal entries
                existingAccountPayable = await CreateAccountPayable(userIdCreatedBy, completeModel, accountPayableAmount, journalEntriesToCreate,
                    listOfMovementsForPayment);

                // Commit the transaction after successful creation
                await transaction.CommitAsync();
                return MethodResponse.CreateSuccessResponse("Reported as account payable successfully!");
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of an error
                await transaction.RollbackAsync();
                return MethodResponse.CreateFailureExceptionResponse(ex.Message);
            }
        }

        private async Task<AccountPayable> CreateAccountPayable(string userIdCreatedBy,
    CreateUpdateConsultantPaymentVM paymentData, decimal accountPayableAmount, List<JournalAccountPayableEntry> journalEntriesToCreate,
    GetListOfMovementsForPaymentVM listOfBenefitsMovements)
        {
            // Get necessary transaction statuses
            var transactionStatuses = await _db.TRANSACTION_STATUSES
                .Where(x => x.Name == "Sent to be paid" || x.Name == "Pending Accounting" || x.Name == "Rejected")
                .ToListAsync();

            // Create the account payable entity
            var accountPayableToCreate = new AccountPayable
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

            // Add the new account payable to the database
            await _db.ACCOUNTS_PAYABLE.AddAsync(accountPayableToCreate);
            await _db.SaveChangesAsync();

            foreach (var movement in listOfBenefitsMovements.ProjectMovements)
            {
                AccountPayableMovement movementToCreate = new()
                {
                    MovementId = movement.MovementId,
                    ProjectId = movement.ProjectId,
                    Description = movement.MovementTypeName,
                    MovementTypeId = movement.MovementTypeId,
                    Type = movement.PaymentType,
                    Quantity = movement.Quantity,
                    UnitPrice = movement.UnitPrice,
                    AccountPayableId = accountPayableToCreate.AccountPayableId
                };
                await _db.ACCOUNTS_PAYABLE_MOVEMENTS.AddAsync(movementToCreate);
                await _db.SaveChangesAsync();
            }
            foreach (var movement in listOfBenefitsMovements.BenefitsAndOtherMovements)
            {
                AccountPayableMovement movementToCreate = new()
                {
                    MovementId = movement.MovementId,
                    ProjectId = movement.ProjectId,
                    Description = movement.MovementTypeName,
                    MovementTypeId = movement.MovementTypeId,
                    Type = movement.PaymentType,
                    Quantity = movement.Quantity,
                    UnitPrice = movement.UnitPrice,
                    AccountPayableId = accountPayableToCreate.AccountPayableId
                };
                await _db.ACCOUNTS_PAYABLE_MOVEMENTS.AddAsync(movementToCreate);
                await _db.SaveChangesAsync();
            }
            foreach (var movement in listOfBenefitsMovements.DebitsMovements)
            {
                AccountPayableMovement movementToCreate = new()
                {
                    MovementId = movement.MovementId,
                    ProjectId = movement.ProjectId,
                    Description = movement.MovementTypeName,
                    MovementTypeId = movement.MovementTypeId,
                    Type = movement.PaymentType,
                    Quantity = movement.Quantity,
                    UnitPrice = movement.UnitPrice,
                    AccountPayableId = accountPayableToCreate.AccountPayableId
                };
                await _db.ACCOUNTS_PAYABLE_MOVEMENTS.AddAsync(movementToCreate);
                await _db.SaveChangesAsync();
            }

            // Retrieve or create a new journal for accounts payable
            var existingJournal = await GetExistingOrCreateJournalAccountPayable(DateTime.Parse(paymentData.StartDatePeriod), DateTime.Parse(paymentData.EndDatePeriod), paymentData.CompanyId,
            userIdCreatedBy);

            // Add journal entries to the database
            await CreateJournalAccountPayableEntries(journalEntriesToCreate,
            existingJournal.JournalId, accountPayableToCreate.AccountPayableId);

            // Update movements statuses based on the period and consultant
            await UpdateMovementsStatuses(transactionStatuses, DateTime.Parse(paymentData.StartDatePeriod), DateTime.Parse(paymentData.EndDatePeriod),
                       (int)paymentData.ConsultantId, "Sent to be paid");

            // Return the newly created account payable
            return accountPayableToCreate;
        }

        private async Task CreateJournalAccountPayableEntries(List<JournalAccountPayableEntry> journalEntriesToCreate,
            int journalParentId, int accountPayableId)
        {
            var mergedJournalEntries = new List<JournalAccountPayableEntry>();

            foreach (var journalEntry in journalEntriesToCreate)
            {
                // Look for an existing entry with the same properties
                var existingEntry = mergedJournalEntries.FirstOrDefault(e =>
                    e.CostCenterId == journalEntry.CostCenterId &&
                    e.AccountingAccountId == journalEntry.AccountingAccountId &&
                    e.Reference == journalEntry.Reference);

                if (existingEntry != null)
                {
                    // If it exists, sum Debit or Credit accordingly
                    if (journalEntry.Debit > 0)
                    {
                        existingEntry.Debit += journalEntry.Debit;
                    }
                    else if (journalEntry.Credit > 0)
                    {
                        existingEntry.Credit += journalEntry.Credit;
                    }
                }
                else
                {
                    // If it doesn't exist, add it to the list of merged entries
                    journalEntry.AccountPayableId = accountPayableId;
                    journalEntry.JournalId = journalParentId;
                    mergedJournalEntries.Add(journalEntry);
                }
            }

            // Now add the merged entries to the database
            foreach (var mergedEntry in mergedJournalEntries)
            {
                await _db.JOURNAL_ACCOUNTS_PAYABLE_ENTRIES.AddAsync(mergedEntry);
            }
            // Save all journal entries
            await _db.SaveChangesAsync();
        }

        private async Task<JournalAccountPayable> GetExistingOrCreateJournalAccountPayable(DateTime startDate, DateTime endDate, string companyId,
            string userActionedBy)
        {
            var existingJournal = await _db.JOURNAL_ACCOUNTS_PAYABLE.Include(x => x.TransactionStatus)
                .FirstOrDefaultAsync(x => x.StartDatePeriod == startDate &&
                x.EndDatePeriod == endDate && x.CompanyId == companyId && x.TransactionStatus.Name == "Pending Accounting");

            if (existingJournal == null)
            {
                // Get the next journal consecutive number
                var journalConsecutive = await _db.GLOBAL_CONSECUTIVES.FirstOrDefaultAsync(x => x.Name == "JOURNAL_CXP" && x.CompanyId == companyId);
                journalConsecutive.ConsecutiveNumber++;

                var statusPendingAccounting = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Pending Accounting");
                // Create a new journal entry
                var journalToCreate = new JournalAccountPayable
                {
                    CompanyId = companyId,
                    TransactionStatusId = statusPendingAccounting.TransactionStatusId,
                    StartDatePeriod = startDate,
                    EndDatePeriod = endDate,
                    Entry = $"{companyId}{journalConsecutive.ConsecutiveNumber.ToString().PadLeft(7, '0')}",
                    AccountingPackage = companyId,
                    EntryType = companyId,
                    AccountingDate = endDate,
                    CreationDate = DateTime.UtcNow,
                    UserCreatedBy = userActionedBy
                };

                // Add the new journal to the database
                await _db.JOURNAL_ACCOUNTS_PAYABLE.AddAsync(journalToCreate);
                await _db.SaveChangesAsync();
                existingJournal = journalToCreate;
            }
            return existingJournal;
        }

        public async Task<MethodResponse> UpdatePayment(string userIdCreatedBy, CreateUpdateConsultantPaymentVM paymentData)
        {
            // Validate if payment data is null
            if (paymentData == null) return MethodResponse.CreateFailureExceptionResponse("Data cannot be null.");

            // Start database transaction
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Retrieve the existing payment
                var existingPayment = await _db.CONSULTANT_PAYMENTS.FirstOrDefaultAsync(x => x.ConsultantPaymentId == paymentData.ConsultantPaymentId);
                if (existingPayment == null) return MethodResponse.CreateFailureExceptionResponse("The payment no longer exists.");

                // Retrieve the account payable
                var existingAccountPayable = await _db.ACCOUNTS_PAYABLE.FirstOrDefaultAsync(x => x.ConsultantId == paymentData.ConsultantId &&
                    x.StartDatePeriod == DateTime.Parse(paymentData.StartDatePeriod) && x.EndDatePeriod == DateTime.Parse(paymentData.EndDatePeriod)
                    && x.Voided == false);
                if (existingAccountPayable == null) return MethodResponse.CreateFailureExceptionResponse("Account payable does not exist.");

                // Calculate new balance
                var updatedBalance = (existingPayment.PaymentAmount + existingAccountPayable.BalanceAmount) - paymentData.PaymentAmount;
                if (paymentData.PaymentAmount > (existingAccountPayable.BalanceAmount + existingPayment.PaymentAmount))
                    return MethodResponse.CreateFailureValidationResponse("The amount to pay must be less than or equal to the account payable balance.");

                // Update the balance
                existingAccountPayable.BalanceAmount = (decimal)updatedBalance;

                // Check for transaction status changes if balance is zero
                var transactionStatuses = await _db.TRANSACTION_STATUSES.Where(x => x.Name == "Paid" || x.Name == "Sent to be paid" || x.Name == "Rejected").ToListAsync();
                if (existingAccountPayable.BalanceAmount == 0)
                {
                    existingAccountPayable.TransactionStatusId = transactionStatuses.First(x => x.Name == "Paid").TransactionStatusId;
                    await UpdateMovementsStatuses(transactionStatuses, DateTime.Parse(paymentData.StartDatePeriod), DateTime.Parse(paymentData.EndDatePeriod), (int)paymentData.ConsultantId, "Paid");
                }
                else
                {
                    existingAccountPayable.TransactionStatusId = transactionStatuses.First(x => x.Name == "Sent to be paid").TransactionStatusId;
                }

                // Check if the 'Pending Accounting' status exists
                var pendingStatus = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Pending Accounting");
                if (pendingStatus == null) return MethodResponse.CreateFailureNotFoundResponse("Pending Accounting status not found.");

                // Retrieve or create the parent book entry
                var existingBookEntryParent = await _db.PAYMENT_BOOK_ENTRIES_PARENT.FirstOrDefaultAsync(x => x.TransactionStatusId == pendingStatus.TransactionStatusId && x.CompanyId == paymentData.CompanyId);
                if (existingBookEntryParent == null)
                {
                    existingBookEntryParent = new PaymentBookEntryParent
                    {
                        TransactionStatusId = pendingStatus.TransactionStatusId,
                        CompanyId = paymentData.CompanyId,
                        CreationDate = DateTime.UtcNow,
                        UserCreatedBy = userIdCreatedBy
                    };
                    await _db.PAYMENT_BOOK_ENTRIES_PARENT.AddAsync(existingBookEntryParent);
                    await _db.SaveChangesAsync();
                }

                // Check if there is an existing child entry associated with the payment
                var existingChildEntry = await (from bc in _db.PAYMENT_BOOK_ENTRIES_CHILD
                                                join bp in _db.PAYMENT_BOOK_ENTRIES_PARENT on bc.ParentId equals bp.ParentId
                                                where bc.ConsultantPaymentId == existingPayment.ConsultantPaymentId && bp.TransactionStatusId == pendingStatus.TransactionStatusId
                                                select bc).FirstOrDefaultAsync();

                // If there is no existing child entry, we need to create a new payment and associate it with a new child entry
                if (existingChildEntry == null)
                {
                    var consultantToPay = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.ConsultantId == paymentData.ConsultantId);
                    if (consultantToPay == null) return MethodResponse.CreateFailureNotFoundResponse("Consultant not found.");

                    var userToPay = await _db.AspNetUsers.FirstOrDefaultAsync(x => x.Id == consultantToPay.UserId);
                    if (userToPay == null) return MethodResponse.CreateFailureNotFoundResponse("User not found.");

                    // Mark the existing payment as voided
                    existingPayment.Voided = true;
                    long uniqueNumber = DateTime.UtcNow.Ticks * 1000 + new Random().Next(1000);
                    existingPayment.ReferenceNumber = $"Voided({existingPayment.ReferenceNumber})" + uniqueNumber;

                    // Create a new payment with updated values
                    var newPayment = new ConsultantPayment
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
                    await _db.CONSULTANT_PAYMENTS.AddAsync(newPayment);
                    await _db.SaveChangesAsync();

                    // Mark the old child entry as voided (if it exists)
                    var oldChildEntry = await _db.PAYMENT_BOOK_ENTRIES_CHILD.FirstOrDefaultAsync(x => x.ConsultantPaymentId == existingPayment.ConsultantPaymentId && x.Voided == false);
                    if (oldChildEntry != null)
                    {
                        oldChildEntry.Voided = true;
                        await _db.SaveChangesAsync();
                    }

                    // Create a new child book entry with the newly created payment
                    var bookEntryChildToCreate = new PaymentBookEntryChild
                    {
                        ParentId = existingBookEntryParent.ParentId,
                        ConsultantPaymentId = newPayment.ConsultantPaymentId,
                        Notes = $"Payment updated to: {userToPay.Name} {userToPay.LastName}",
                        Voided = false
                    };
                    await _db.PAYMENT_BOOK_ENTRIES_CHILD.AddAsync(bookEntryChildToCreate);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    // Update existing payment details
                    existingPayment.ReferenceNumber = paymentData.ReferenceNumber;
                    existingPayment.PaymentMethodId = (int)paymentData.PaymentMethodId;
                    existingPayment.PaymentAmount = (decimal)paymentData.PaymentAmount;
                    existingPayment.CompanyId = paymentData.CompanyId;
                    existingPayment.BankAccountId = (int)paymentData.BankAccountId;
                    existingPayment.AccountingDate = DateTime.Parse(paymentData.AccountingDate);
                    existingPayment.UserLastUpdatedBy = userIdCreatedBy;
                    existingPayment.LastUpdatedDate = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }

                // Commit the transaction
                await transaction.CommitAsync();
                return MethodResponse.CreateSuccessResponse("Payment updated successfully!");
            }
            catch (DbUpdateException ex)
            {
                // Rollback the transaction in case of an error
                await transaction.RollbackAsync();

                // Check if the exception is caused by a unique constraint violation
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601) // SqlException error code 2601 is for duplicate keys
                {
                    return MethodResponse.CreateFailureValidationResponse("The reference number already exists for the selected Bank Account. Please use a different one.");
                }
                else
                {
                    // Handle other exceptions
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                }
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of error
                await transaction.RollbackAsync();
                return MethodResponse.CreateFailureExceptionResponse(ex.Message);
            }
        }

        public async Task<MethodResponse> DeletePayment(int paymentId)
        {
            // Start database transaction
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Retrieve the existing payment by ID
                var existingPayment = await _db.CONSULTANT_PAYMENTS.FirstOrDefaultAsync(x => x.ConsultantPaymentId == paymentId);
                if (existingPayment == null) return MethodResponse.CreateFailureNotFoundResponse("The payment no longer exists.");

                // Retrieve the account payable associated with the payment
                var existingAccountPayable = await _db.ACCOUNTS_PAYABLE.FirstOrDefaultAsync(x => x.AccountPayableId == existingPayment.AccountPayableId);
                if (existingAccountPayable == null) return MethodResponse.CreateFailureNotFoundResponse("The account payable no longer exists.");

                // If the balance is zero, update movement statuses
                if (existingAccountPayable.BalanceAmount == 0)
                {
                    var transactionStatuses = await _db.TRANSACTION_STATUSES.Where(x => x.Name == "Sent to be paid" || x.Name == "Rejected").ToListAsync();
                    await UpdateMovementsStatuses(transactionStatuses, existingAccountPayable.StartDatePeriod, existingAccountPayable.EndDatePeriod, existingAccountPayable.ConsultantId, "Sent to be paid");
                    existingAccountPayable.TransactionStatusId = transactionStatuses.FirstOrDefault(x => x.Name == "Sent to be paid").TransactionStatusId;
                }

                // Revert the balance by adding the payment amount back to the account payable
                existingAccountPayable.BalanceAmount += existingPayment.PaymentAmount;

                // Mark the payment as voided
                existingPayment.Voided = true;
                long uniqueNumber = DateTime.UtcNow.Ticks * 1000 + new Random().Next(1000);
                existingPayment.ReferenceNumber = $"Voided({existingPayment.ReferenceNumber})" + uniqueNumber;
                await _db.SaveChangesAsync();

                // Handle any associated book entries
                var pendingStatus = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Pending Accounting");
                if (pendingStatus == null) return MethodResponse.CreateFailureNotFoundResponse("Pending Accounting status not found.");

                var existingChildEntry = await (from bc in _db.PAYMENT_BOOK_ENTRIES_CHILD
                                                join bp in _db.PAYMENT_BOOK_ENTRIES_PARENT on bc.ParentId equals bp.ParentId
                                                where bc.ConsultantPaymentId == existingPayment.ConsultantPaymentId
                                                && bp.TransactionStatusId != pendingStatus.TransactionStatusId
                                                select bc).FirstOrDefaultAsync();

                if (existingChildEntry != null)
                {
                    // Mark child entry as voided
                    var childMovementToAvoid = await _db.PAYMENT_BOOK_ENTRIES_CHILD.FirstOrDefaultAsync(x => x.ChildId == existingChildEntry.ChildId);
                    childMovementToAvoid.Voided = true;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    // Remove child entry if it has not been registered yet
                    var existingChildEntryNotRegister = await (from bc in _db.PAYMENT_BOOK_ENTRIES_CHILD
                                                               join bp in _db.PAYMENT_BOOK_ENTRIES_PARENT on bc.ParentId equals bp.ParentId
                                                               where bc.ConsultantPaymentId == existingPayment.ConsultantPaymentId
                                                               && bp.TransactionStatusId == pendingStatus.TransactionStatusId
                                                               select bc).FirstOrDefaultAsync();
                    var childToDelete = await _db.PAYMENT_BOOK_ENTRIES_CHILD.FirstOrDefaultAsync(x => x.ChildId == existingChildEntryNotRegister.ChildId);
                    _db.PAYMENT_BOOK_ENTRIES_CHILD.Remove(childToDelete);
                    await _db.SaveChangesAsync();
                }

                // Commit the transaction
                await transaction.CommitAsync();
                return MethodResponse.CreateSuccessResponse("The payment was deleted!");
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of an error
                await transaction.RollbackAsync();
                return MethodResponse.CreateFailureExceptionResponse(ex.Message);
            }
        }

        public async Task<List<GetConsultantPaymentsInPeriodVM>> GetConsultantPaymentsInPeriod(int consultantId, DateTime startDate,
            DateTime endDate)
        {
            var result = await (from cp in _db.CONSULTANT_PAYMENTS
                                join pm in _db.PAYMENT_METHODS on cp.PaymentMethodId equals pm.PaymentMethodId
                                join ba in _db.BANK_ACCOUNTS on cp.BankAccountId equals ba.BankAccountId
                                where cp.ConsultantId == consultantId && (cp.StartDatePeriod >= startDate && cp.EndDatePeriod <= endDate)
                                && cp.Voided == false
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

        public decimal GetConsultantTotalAmountToPay(GetListOfMovementsForPaymentVM? listOfMovements)
        {
            decimal totalAmountToPay = 0;

            if (listOfMovements != null)
            {
                foreach (var property in listOfMovements.GetType().GetProperties())
                {
                    if (property.GetValue(listOfMovements) is IEnumerable<object> list && list.Any())
                    {
                        foreach (var item in list)
                        {
                            if (item is GetPaymentDetailsMovementsVM movement)
                            {
                                if (property.Name != "DebitsMovements")
                                {
                                    totalAmountToPay += movement.TotalAmount;
                                }
                                else
                                {
                                    totalAmountToPay -= movement.TotalAmount;
                                }
                            }
                        }
                    }
                }
            }
            return totalAmountToPay;
        }

        public async Task<MethodResponse> ApproveAndRejectSubmission(string userIdCreatedBy, ApproveRejectSubmissionVM dataFromUser,
            string baseUrl)
        {
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var submission = await _db.REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS
                        .Include(x => x.Project).FirstOrDefaultAsync(x => x.SubmissionId == dataFromUser.SubmissionId);
                    if (submission == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Submission does not exist.");
                    }

                    var consultant = await _consultantDetailRepository.GetConsultantWithUserAsync(submission.ConsultantId);
                    if (consultant == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Consultant does not exist.");
                    }

                    var userActionedByObject = await _db.AspNetUsers.FirstOrDefaultAsync(x => x.Id == userIdCreatedBy);
                    if (userActionedByObject == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("User actioned by does not exist.");
                    }

                    var movements = await _db.REPORTING_MY_TIME_MOVEMENTS.Where(x => x.ProjectId == submission.ProjectId &&
                    x.ConsultantId == submission.ConsultantId && (x.ActionDate >= submission.StartPeriodDate &&
                    x.ActionDate <= submission.EndPeriodDate)).ToListAsync();

                    var transactionStatusFromDb = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == dataFromUser.TransactionStatus);
                    if (transactionStatusFromDb == null)
                    {
                        return MethodResponse.CreateFailureNotFoundResponse("Transaction status '" + dataFromUser.TransactionStatus + "' not found.");
                    }

                    foreach (var repMovement in movements)
                    {
                        repMovement.TransactionStatusId = transactionStatusFromDb.TransactionStatusId;
                    }

                    submission.TransactionStatusId = transactionStatusFromDb.TransactionStatusId;

                    if (dataFromUser.TransactionStatus == "Rejected")
                    {
                        var commentToCreate = new ReportingMyTimeComments
                        {
                            ConsultantId = submission.ConsultantId,
                            ProjectId = submission.ProjectId,
                            Body = dataFromUser.Body,
                            CreationDate = DateTime.UtcNow,
                            ActionDate = submission.EndPeriodDate,
                            UserId = userIdCreatedBy,
                            SubmissionId = submission.SubmissionId
                        };
                        await _db.REPORTING_MY_TIME_COMMENTS.AddAsync(commentToCreate);

                        await UpdateAccountsPayableStatusWhenChangesAsync(submission.StartPeriodDate, submission.EndPeriodDate,
                            submission.ConsultantId);
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    //Send notification 


                    try
                    {
                        DateTime startDateTime = submission.StartPeriodDate;
                        DateTime endDateTime = submission.EndPeriodDate;
                        string startDateFormated = startDateTime.ToString("MMM d", System.Globalization.CultureInfo.InvariantCulture);
                        string endDateFormated = endDateTime.ToString("MMM d", System.Globalization.CultureInfo.InvariantCulture);
                        string periodString = $"{startDateFormated} - {endDateFormated}";

                        var emailToSend = PrepareEmailContentApproveRejectSubmission(consultant.Name, consultant.Email, periodString,
            submission.Project.Name, dataFromUser.TransactionStatus, userActionedByObject.Name + " " + userActionedByObject.LastName, dataFromUser.Body, baseUrl + "/TrackingTool/ReportingMyTime");
                        string message = JsonConvert.SerializeObject(emailToSend);

                        await _queueClient.Value.SendMessageAsync(StringsMethods.Base64Encode(message));

                    }
                    catch (Exception ex)
                    {

                    }

                    return MethodResponse.CreateSuccessResponse("You have " + dataFromUser.TransactionStatus + " the submission!");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                }
            }
        }

        private SendEmailVM PrepareEmailContentApproveRejectSubmission(string consultantName, string consultantEmail, string period,
            string projectName, string status, string userActionedBy, string? rejectedComment, string baseUrl)
        {
            string buttonUrl = $"{baseUrl}/TrackingTool/ReportingMyTime";

            var emailTemplates = new EmailTemplates();
            var emailBody = emailTemplates.ApprovedRejectedSubmissionBody(buttonUrl, consultantName.Trim(), period, projectName, status,
                userActionedBy, rejectedComment);
            string bodyTitle = status == "Approved" ? "TIMESHEET APPROVED" : "TIMESHEET REJECTED";
            string emailTitle = status == "Approved" ? "YOUR TIMESHEET WAS APPROVED" : "YOUR TIMESHEET WAS REJECTED";
            var templateEmail = emailTemplates.EmailTemplate(bodyTitle, emailBody);

            return new SendEmailVM
            {
                Subject = $"{emailTitle} - RIPPLE BY OCEANS",
                SharedEmailFrom = _config["SharedMailboxEmailRippleApp"],
                EmailTo = consultantEmail.Trim(),
                Body = templateEmail
            };
        }

        private async Task UpdateAccountsPayableStatusWhenChangesAsync(DateTime startDate, DateTime endDate, int consultantId)
        {
            try
            {
                var existingAccountPayable = await _db.ACCOUNTS_PAYABLE.Include(x => x.TransactionStatus).FirstOrDefaultAsync(x =>
x.StartDatePeriod == startDate && x.EndDatePeriod == endDate
&& x.ConsultantId == consultantId && x.Voided == false);

                if (existingAccountPayable != null)
                {
                    if (existingAccountPayable.TransactionStatus.Name != "Updated - Pending Review")
                    {
                        var transactionStatusPendingReview = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Updated - Pending Review");
                        existingAccountPayable.TransactionStatusId = transactionStatusPendingReview.TransactionStatusId;
                        await _db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool?> AccountPayableIsAccountedAsync(int accountPayableId)
        {
            var entry = await _db.JOURNAL_ACCOUNTS_PAYABLE_ENTRIES
                                 .FirstOrDefaultAsync(x => x.AccountPayableId == accountPayableId);

            if (entry == null) return null;

            var existingAccountPayable = await _db.JOURNAL_ACCOUNTS_PAYABLE
                                                  .Include(x => x.TransactionStatus)
                                                  .FirstOrDefaultAsync(x => x.JournalId == entry.JournalId);

            return existingAccountPayable?.TransactionStatus.Name == "Accounted" ? true : (bool?)false;
        }
        public async Task<bool> ExistsPaymentForAccountPayableAsync(int accountPayableId)
        {
            var payment = await _db.CONSULTANT_PAYMENTS
                     .Where(x => x.AccountPayableId == accountPayableId && x.Voided == false)
                     .OrderByDescending(x => x.EndDatePeriod)
                     .FirstOrDefaultAsync();

            if (payment == null) return false;

            return true;
        }

        public async Task<MethodResponse> FixDifferenceToMayPaymentAsync(int consultantId, DateTime startDate, DateTime endDate,
            string userActionedBy)
        {
            var consultant = await _consultantDetailRepository.GetConsultantWithUserAsync(consultantId);

            if (consultant == null) return MethodResponse.CreateFailureNotFoundResponse("The Consultant was not found");

            var existingAccountPayable = await _db.ACCOUNTS_PAYABLE.FirstOrDefaultAsync(x => x.ConsultantId == consultant.ConsultantId &&
            x.StartDatePeriod == startDate && x.EndDatePeriod == endDate && x.Voided == false);

            if (existingAccountPayable == null) return MethodResponse.CreateFailureNotFoundResponse("The payment details doen't have an account payable.");

            bool? accountPayableAccuntedStatus = await AccountPayableIsAccountedAsync(existingAccountPayable.AccountPayableId);

            if (accountPayableAccuntedStatus == null) return MethodResponse.CreateFailureNotFoundResponse("The Accounted status was not found");

            var movementsListFromDb = await GetMovementsToPay(consultant, startDate, endDate);

            decimal totalAmountToPay = GetConsultantTotalAmountToPay((GetListOfMovementsForPaymentVM?)movementsListFromDb.GenericList);

            var journalEntriesToCreate = await GetJournalEntriesReadyToCreate((GetListOfMovementsForPaymentVM)movementsListFromDb.GenericList,
consultantId, consultant.CompanyId, endDate, totalAmountToPay);

            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var accountPayable = existingAccountPayable;
                    var statusSentToBePaid = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Sent to be paid");

                    if (statusSentToBePaid == null) return MethodResponse.CreateFailureNotFoundResponse("The status 'Sent to be paid' was not found");

                    if ((bool)accountPayableAccuntedStatus) //If account payable is already accounted
                    {
                        //Void the existing account payable
                        existingAccountPayable.Voided = true;
                        await _db.SaveChangesAsync();

                        //Create new Account Payable
                        AccountPayable newAccountPayable = new()
                        {
                            ConsultantId = existingAccountPayable.ConsultantId,
                            StartDatePeriod = existingAccountPayable.StartDatePeriod,
                            EndDatePeriod = existingAccountPayable.EndDatePeriod,
                            AccountingDate = existingAccountPayable.AccountingDate,
                            Amount = existingAccountPayable.Amount,
                            BalanceAmount = existingAccountPayable.BalanceAmount,
                            CreationDate = DateTime.UtcNow,
                            UserCreatedBy = userActionedBy,
                            CompanyId = existingAccountPayable.CompanyId,
                            TransactionStatusId = existingAccountPayable.TransactionStatusId
                        };
                        await _db.ACCOUNTS_PAYABLE.AddAsync(newAccountPayable);
                        await _db.SaveChangesAsync();
                        accountPayable = newAccountPayable;

                        //Create new journal entries
                        var existingOrNewJournal = await GetExistingOrCreateJournalAccountPayable(startDate, endDate, consultant.CompanyId,
            userActionedBy);

                        var journalEntriesToCreateReverse = await _db.JOURNAL_ACCOUNTS_PAYABLE_ENTRIES
                            .Where(x => x.AccountPayableId == existingAccountPayable.AccountPayableId).ToListAsync();

                        List<JournalAccountPayableEntry> journalEntriesToCreateReverseList = new();
                        foreach (var entryToReverse in journalEntriesToCreateReverse)
                        {
                            JournalAccountPayableEntry newEntry = new()
                            {
                                CostCenterId = entryToReverse.CostCenterId,
                                AccountingAccountId = entryToReverse.AccountingAccountId,
                                Reference = entryToReverse.Reference,
                                Debit = entryToReverse.Credit,
                                Credit = entryToReverse.Debit,
                                AccountPayableId = existingAccountPayable.AccountPayableId,
                                JournalId = existingOrNewJournal.JournalId
                            };
                            journalEntriesToCreateReverseList.Add(newEntry);
                        }

                        await CreateJournalAccountPayableEntries(journalEntriesToCreateReverseList,
            existingOrNewJournal.JournalId, existingAccountPayable.AccountPayableId);

                        // Update AccountPayableId to payments if exists
                        var payments = await _db.CONSULTANT_PAYMENTS
                            .Where(x => x.AccountPayableId == existingAccountPayable.AccountPayableId)
                            .ToListAsync();
                        foreach (var payment in payments)
                        {
                            payment.AccountPayableId = accountPayable.AccountPayableId;
                            await _db.SaveChangesAsync();
                        }
                    }
                    else //If account payable is no accounted
                    {
                        //Remove old accounts payable movements
                        var existingAccountPayableMovements = await _db.ACCOUNTS_PAYABLE_MOVEMENTS
                            .Where(x => x.AccountPayableId == accountPayable.AccountPayableId).ToListAsync();

                        foreach (var existingMovement in existingAccountPayableMovements)
                        {
                            _db.ACCOUNTS_PAYABLE_MOVEMENTS.Remove(existingMovement);
                        }
                        await _db.SaveChangesAsync();

                        //Remove old journal entries
                        var existingJournalEntries = await _db.JOURNAL_ACCOUNTS_PAYABLE_ENTRIES
                            .Where(x => x.AccountPayableId == accountPayable.AccountPayableId).ToListAsync();

                        foreach (var entryToDelete in existingJournalEntries)
                        {
                            _db.JOURNAL_ACCOUNTS_PAYABLE_ENTRIES.Remove(entryToDelete);
                        }
                        await _db.SaveChangesAsync();
                    }

                    //Account payable must be updated
                    if (totalAmountToPay > accountPayable.Amount)
                    {
                        accountPayable.BalanceAmount += (totalAmountToPay - accountPayable.Amount);
                    }
                    else
                    {
                        accountPayable.BalanceAmount -= (accountPayable.Amount - totalAmountToPay);
                    }
                    accountPayable.Amount = totalAmountToPay;
                    accountPayable.TransactionStatusId = statusSentToBePaid.TransactionStatusId;
                    accountPayable.LastUpdatedDate = DateTime.UtcNow;
                    accountPayable.UserLastUpdatedBy = userActionedBy;
                    await _db.SaveChangesAsync();

                    //Create new accounts payable movements

                    GetListOfMovementsForPaymentVM movementsToPayList = (GetListOfMovementsForPaymentVM)movementsListFromDb.GenericList;
                    foreach (var movement in movementsToPayList.ProjectMovements)
                    {
                        AccountPayableMovement movementToCreate = new()
                        {
                            MovementId = movement.MovementId,
                            ProjectId = movement.ProjectId,
                            Description = movement.MovementTypeName,
                            MovementTypeId = movement.MovementTypeId,
                            Type = movement.PaymentType,
                            Quantity = movement.Quantity,
                            UnitPrice = movement.UnitPrice,
                            AccountPayableId = accountPayable.AccountPayableId
                        };
                        await _db.ACCOUNTS_PAYABLE_MOVEMENTS.AddAsync(movementToCreate);
                    }
                    foreach (var movement in movementsToPayList.BenefitsAndOtherMovements)
                    {
                        AccountPayableMovement movementToCreate = new()
                        {
                            MovementId = movement.MovementId,
                            ProjectId = movement.ProjectId,
                            Description = movement.MovementTypeName,
                            MovementTypeId = movement.MovementTypeId,
                            Type = movement.PaymentType,
                            Quantity = movement.Quantity,
                            UnitPrice = movement.UnitPrice,
                            AccountPayableId = accountPayable.AccountPayableId
                        };
                        await _db.ACCOUNTS_PAYABLE_MOVEMENTS.AddAsync(movementToCreate);
                    }
                    foreach (var movement in movementsToPayList.DebitsMovements)
                    {
                        AccountPayableMovement movementToCreate = new()
                        {
                            MovementId = movement.MovementId,
                            ProjectId = movement.ProjectId,
                            Description = movement.MovementTypeName,
                            MovementTypeId = movement.MovementTypeId,
                            Type = movement.PaymentType,
                            Quantity = movement.Quantity,
                            UnitPrice = movement.UnitPrice,
                            AccountPayableId = accountPayable.AccountPayableId
                        };
                        await _db.ACCOUNTS_PAYABLE_MOVEMENTS.AddAsync(movementToCreate);
                    }
                    await _db.SaveChangesAsync();

                    //Create new journal entries
                    var existingOrCreatedJournal = await GetExistingOrCreateJournalAccountPayable(startDate, endDate, consultant.CompanyId,
        userActionedBy);

                    await CreateJournalAccountPayableEntries(journalEntriesToCreate,
        existingOrCreatedJournal.JournalId, accountPayable.AccountPayableId);

                    await transaction.CommitAsync();
                    return new MethodResponse { Success = true, Message = "The account payable was fixed" };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                }
            }
        }

        public async Task<GetDataForDeferToNextPeriodVM> GetMovementsToDeferAsync(int consultantId, DateTime startDate, DateTime endDate)
        {
            var consultant = await _consultantDetailRepository.GetConsultantWithUserAsync(consultantId);

            var existingAccountPayable = await _db.ACCOUNTS_PAYABLE
                .FirstOrDefaultAsync(x => x.ConsultantId == consultant.ConsultantId &&
            x.StartDatePeriod == startDate && x.EndDatePeriod == endDate && x.Voided == false);

            var movementsListFromDb = await GetMovementsToPay(consultant, startDate, endDate);
            GetListOfMovementsForPaymentVM movementsToPayList = (GetListOfMovementsForPaymentVM)movementsListFromDb.GenericList;

            decimal totalAmountToPay = GetConsultantTotalAmountToPay((GetListOfMovementsForPaymentVM?)movementsListFromDb.GenericList);

            var existingAccountPayableMovements = await _db.ACCOUNTS_PAYABLE_MOVEMENTS
            .Where(x => x.AccountPayableId == existingAccountPayable.AccountPayableId)
            .OrderBy(x => x.Type).ToListAsync();

            List<GetAccountPayableMovementVM> mergedAllMovementsList = new();

            List<GetAccountPayableMovementVM> mergedPaidMovementsList = new();

            foreach (var realMovement in movementsToPayList.ProjectMovements)
            {
                GetAccountPayableMovementVM newMovement = new()
                {
                    ProjectId = (int)realMovement.ProjectId,
                    Quantity = realMovement.Quantity,
                    TotalAmount = realMovement.TotalAmount,
                    Type = realMovement.PaymentType,
                    MovementTypeId = realMovement.MovementTypeId
                };
                mergedAllMovementsList.Add(newMovement);
            }
            foreach (var realMovement in movementsToPayList.BenefitsAndOtherMovements)
            {
                GetAccountPayableMovementVM newMovement = new()
                {
                    ProjectId = (int)realMovement.ProjectId,
                    Quantity = realMovement.Quantity,
                    TotalAmount = realMovement.TotalAmount,
                    Type = realMovement.PaymentType,
                    MovementTypeId = realMovement.MovementTypeId
                };
                mergedAllMovementsList.Add(newMovement);
            }
            foreach (var realMovement in movementsToPayList.DebitsMovements)
            {
                GetAccountPayableMovementVM newMovement = new()
                {
                    ProjectId = (int)realMovement.ProjectId,
                    Quantity = realMovement.Quantity,
                    TotalAmount = realMovement.TotalAmount,
                    Type = realMovement.PaymentType,
                    MovementTypeId = realMovement.MovementTypeId
                };
                mergedAllMovementsList.Add(newMovement);
            }
            //Paid movements
            foreach (var paidMovement in existingAccountPayableMovements)
            {
                GetAccountPayableMovementVM newMovement = new()
                {
                    ProjectId = (int)paidMovement.ProjectId,
                    Quantity = paidMovement.Quantity,
                    TotalAmount = (paidMovement.Quantity * paidMovement.UnitPrice),
                    Type = paidMovement.Type,
                    MovementTypeId = paidMovement.MovementTypeId
                };
                mergedPaidMovementsList.Add(newMovement);
            }

            mergedAllMovementsList = mergedAllMovementsList.OrderBy(x => x.Type).ToList();
            mergedPaidMovementsList = mergedPaidMovementsList.OrderBy(x => x.Type).ToList();

            // Define a tolerance for the differences (e.g., 0.01 for small differences)
            decimal tolerance = 0.0001m;

            // Step 1: Group and sum within mergedAllMovementsList
            var groupedAllMovementsList = mergedAllMovementsList
                .GroupBy(x => new { x.MovementTypeId, x.Type }) // Group by MovementTypeId and Type
                .Select(g => new GetAccountPayableMovementVM
                {
                    MovementTypeId = g.Key.MovementTypeId,
                    Type = g.Key.Type,
                    Quantity = g.Sum(x => x.Quantity),
                    TotalAmount = g.Sum(x => x.TotalAmount),
                    ProjectId = g.First().ProjectId // Include ProjectId as part of the data
                })
                .ToList();

            // Step 2: Group and sum within mergedPaidMovementsList
            var groupedPaidMovementsList = mergedPaidMovementsList
                .GroupBy(x => new { x.MovementTypeId, x.Type }) // Group by MovementTypeId and Type
                .Select(g => new GetAccountPayableMovementVM
                {
                    MovementTypeId = g.Key.MovementTypeId,
                    Type = g.Key.Type,
                    Quantity = g.Sum(x => x.Quantity),
                    TotalAmount = g.Sum(x => x.TotalAmount),
                    ProjectId = g.First().ProjectId // Include ProjectId as part of the data
                })
                .ToList();

            // Step 3: Find the differences between the two lists
            var differencesList = (from movementAll in groupedAllMovementsList
                                   join movementPaid in groupedPaidMovementsList
                                   on new { MovementTypeId = movementAll.MovementTypeId, Type = movementAll.Type }
                                   equals new { MovementTypeId = movementPaid.MovementTypeId, Type = movementPaid.Type }
                                   into matchedMovements
                                   from movementPaid in matchedMovements.DefaultIfEmpty() // Handle when there's no match
                                   where movementPaid == null // Include if not found in the paid list
                                   || Math.Abs(movementAll.TotalAmount - movementPaid.TotalAmount) > tolerance // Ignore small differences
                                   select new GetAccountPayableMovementVM
                                   {
                                       MovementTypeId = movementAll.MovementTypeId,

                                       // Leave as Debit if it's already Debit, otherwise adjust based on the difference
                                       Type = movementAll.Type == "Debit"
                                              ? "Debit" // Leave as Debit if it's already Debit
                                              : (movementPaid == null || movementAll.TotalAmount > movementPaid.TotalAmount
                                                ? "Credit"  // Difference is in groupedAllMovementsList (movementAll has more)
                                                : "Debit"), // Difference is in groupedPaidMovementsList (movementPaid has more)

                                       Quantity = movementPaid != null
                                                  ? movementAll.Quantity - movementPaid.Quantity // Now we are keeping the sign of the difference
                                                  : movementAll.Quantity, // Keep original Quantity if no match

                                       TotalAmount = movementPaid != null
                                                     ? movementAll.TotalAmount - movementPaid.TotalAmount // Keep the sign of the difference
                                                     : movementAll.TotalAmount, // Keep original TotalAmount if no match

                                       ProjectId = movementAll.ProjectId // Include ProjectId, but don't use it for the comparison
                                   }).ToList();

            decimal differenceAmount = totalAmountToPay - existingAccountPayable.Amount;
            List<ListOfMovementsToDeferToNextPeriodVM> listOfMovementsToReturn = new();

            int index = 0;
            foreach (var difference in differencesList)
            {
                var currentHistory = await _projectConsultantAssignedHistoryRepository.GetCurrentProjectConsultantHistoryAsync(
                    consultantId, difference.ProjectId, endDate);

                var accountingConfig = difference.MovementTypeId == null ? null : await _db.CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION.FirstOrDefaultAsync(x => x.PositionId
                == currentHistory.PositionId && x.CompanyId == consultant.CompanyId && x.MovementTypeId == difference.MovementTypeId);

                string transactionTypeName = "Credit";
                string detail = "";

                if (difference.MovementTypeId == null)
                {
                    transactionTypeName = difference.Type;
                    detail = $"({difference.Type}) not paid in period {startDate.ToString("MM/dd/yyyy")} - {endDate.ToString("MM/dd/yyyy")}";
                }
                if (difference.MovementTypeId != null && differenceAmount < tolerance)
                {
                    transactionTypeName = "Debit";
                }
                if (difference.MovementTypeId != null)
                {
                    var movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.FirstOrDefaultAsync(x => x.MovementTypeId
                    == difference.MovementTypeId);
                    detail = $"({movementType.Name}) not paid in period {startDate.ToString("MM/dd/yyyy")} - {endDate.ToString("MM/dd/yyyy")}";
                }

                var costCenter = accountingConfig == null ? null : await _db.COST_CENTER.FirstOrDefaultAsync(x => x.CostCenterId == accountingConfig.CostCenterId);
                var accountingAccount = accountingConfig == null ? null : await _db.ACCOUNTING_ACCOUNT.FirstOrDefaultAsync(x => x.AccountingAccountId == accountingConfig.AccountingAccountId);

                ListOfMovementsToDeferToNextPeriodVM finalItemToAdd = new()
                {
                    Id = index,
                    CostCenterId = accountingConfig == null ? null : accountingConfig.CostCenterId,
                    CostCenterName = accountingConfig == null ? null : $"({costCenter.CostCenterCode}) {costCenter.Description}",
                    AccountingAccountId = accountingConfig == null ? null : accountingConfig.AccountingAccountId,
                    AccountingAccountName = accountingConfig == null ? null : $"({accountingAccount.AccountingAccountCode}) {accountingAccount.Description}",
                    TransactionTypeName = difference.Type,
                    Quantity = Math.Abs(difference.Quantity),
                    Amount = Math.Abs((difference.TotalAmount / difference.Quantity)),
                    Detail = detail
                };

                listOfMovementsToReturn.Add(finalItemToAdd);
                index++;
            }
            DateTime firstEmptyPeriod = FindFirstEmptyPeriod(consultant.PaymentPeriod, endDate, consultant.ConsultantId);

            GetDataForDeferToNextPeriodVM dataToReturn = new()
            {
                ActionDate = firstEmptyPeriod,
                CompanyId = consultant.CompanyId,
                ListOfMovementsToDefer = listOfMovementsToReturn
            };

            return dataToReturn;
        }

        public DateTime FindFirstEmptyPeriod(int periodId, DateTime endDate, int consultantId)
        {
            DateTime currentStartDate = endDate.AddDays(1); // Start searching the day after endDate

            while (true)
            {
                DateTime currentEndDate = GetPeriodEndDate(periodId, currentStartDate); // Get the appropriate end date for the period

                if (!HasRecordsInRange(currentStartDate, currentEndDate, consultantId))
                {
                    return currentStartDate; // No records found, return the start date of this period
                }

                // Move to the next period (quincena or month)
                currentStartDate = GetNextPeriodStartDate(periodId, currentStartDate);
            }
        }

        // Helper method to get the period's end date based on the period type (quincenal or monthly)
        private DateTime GetPeriodEndDate(int periodId, DateTime startDate)
        {
            if (periodId == 1) // Quincenal
            {
                return (startDate.Day <= 15)
                    ? new DateTime(startDate.Year, startDate.Month, 15) // First half of the month (1-15)
                    : new DateTime(startDate.Year, startDate.Month, DateTime.DaysInMonth(startDate.Year, startDate.Month)); // Second half of the month (16-end)
            }
            // Monthly: Return the last day of the month
            return new DateTime(startDate.Year, startDate.Month, DateTime.DaysInMonth(startDate.Year, startDate.Month));
        }

        // Helper method to move to the next period start date
        private DateTime GetNextPeriodStartDate(int periodId, DateTime startDate)
        {
            if (periodId == 1) // Quincenal
            {
                return (startDate.Day <= 15)
                    ? new DateTime(startDate.Year, startDate.Month, 16) // Move to the second half of the month (16-end)
                    : startDate.AddMonths(1).AddDays(-startDate.Day + 1); // Move to the first day of the next month
            }
            // Monthly: Move to the first day of the next month
            return startDate.AddMonths(1).AddDays(-startDate.Day + 1);
        }

        // Helper method to check if there are records within the given date range
        private bool HasRecordsInRange(DateTime startDate, DateTime endDate, int consultantId)
        {
            return _db.CONSULTANT_PAYMENTS.Any(x => x.StartDatePeriod >= startDate && x.EndDatePeriod <= endDate
            && x.ConsultantId == consultantId);
        }

        public async Task<bool> ValidateConsultantPaymentByDateAsync(DateTime actionDate, int consultantId)
        {
            return await _db.CONSULTANT_PAYMENTS
                .AnyAsync(cp => (cp.StartDatePeriod.Date <= actionDate.Date && cp.EndDatePeriod.Date >= actionDate.Date)
                && cp.ConsultantId == consultantId && cp.Voided == false);
        }


    }
}
