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
                                    ProjectName = project.ProjectName,
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
                                    ProjectName = project.ProjectName,
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
                        GetPaymentDetailsMovementsVM paymentProjectMovement = new()
                        {
                            PaymentType = "Hours/normal payment",
                            ProjectName = project.ProjectName,
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
                    foreach (var holiday in holidays)
                    {
                        GetPaymentDetailsMovementsVM holidayMovement = new()
                        {
                            PaymentType = "Holidays",
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
                    PaymentType = "Reimbursed Benefits",
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
                    PaymentType = "Interviews",
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
                        PaymentType = "Debit/Credit",
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
                        PaymentType = "Debit/Credit",
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
            CreateUpdateConsultantPaymentVM paymentData, decimal accountPayableAmount)
        {
            if (paymentData == null)
            {
                return MethodResponse.CreateFailureExceptionResponse("Data cannot be null.");
            }
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingAccountPayable = await _db.ACCOUNTS_PAYABLE.FirstOrDefaultAsync(x => x.ConsultantId == paymentData.ConsultantId &&
                    x.StartDatePeriod == DateTime.Parse(paymentData.StartDatePeriod) && x.EndDatePeriod == DateTime.Parse(paymentData.EndDatePeriod));

                    var existingJournal = await _db.JOURNAL_ACCOUNTS_PAYABLE.FirstOrDefaultAsync(x => x.StartDatePeriod == DateTime.Parse(paymentData.StartDatePeriod) &&
                        x.EndDatePeriod == DateTime.Parse(paymentData.EndDatePeriod));
                    if (existingJournal == null)
                    {
                        var transactionStatusPending = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Pending to register");
                        var journalConsecutive = await _db.GLOBAL_CONSECUTIVES.FirstOrDefaultAsync(x => x.Name == "JOURNAL_CXP" && x.CompanyId == paymentData.CompanyId);
                        if (journalConsecutive == null)
                        {
                            return MethodResponse.CreateFailureExceptionResponse("Consecutive does not exist.");
                        }
                        journalConsecutive.ConsecutiveNumber++;
                        JournalAccountPayable journalToCreate = new()
                        {
                            CompanyId = paymentData.CompanyId,
                            TransactionStatusId = transactionStatusPending.TransactionStatusId,
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
                    //Create the account payable movement
                    if (existingAccountPayable == null)
                    {
                        var transactionStatus = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Sent to be paid");

                        AccountPayable accountPayableToCreate = new()
                        {
                            ConsultantId = (int)paymentData.ConsultantId,
                            StartDatePeriod = DateTime.Parse(paymentData.StartDatePeriod),
                            EndDatePeriod = DateTime.Parse(paymentData.EndDatePeriod),
                            AccountingDate = DateTime.Parse(paymentData.AccountingDate),
                            Amount = accountPayableAmount,
                            BalanceAmount = accountPayableAmount,
                            CreationDate = DateTime.UtcNow,
                            UserCreatedBy = userIdCreatedBy,
                            CompanyId = paymentData.CompanyId,
                            TransactionStatusId = transactionStatus.TransactionStatusId
                        };

                        await _db.ACCOUNTS_PAYABLE.AddAsync(accountPayableToCreate);
                        await _db.SaveChangesAsync();
                        existingAccountPayable = accountPayableToCreate;
                    }
                    if (paymentData.PaymentAmount > existingAccountPayable.BalanceAmount)
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
                        var transactionStatusPaid = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Paid");
                        existingAccountPayable.TransactionStatusId = transactionStatusPaid.TransactionStatusId;
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
                    if (paymentData.PaymentAmount > (existingAccountPayable.BalanceAmount + (decimal)existingPayment.PaymentAmount))
                    {
                        return MethodResponse.CreateFailureValidationResponse($"The amount to pay must be less than or equal to the account payable balance amount.");
                    }

                    existingAccountPayable.BalanceAmount = (existingPayment.PaymentAmount + (decimal)existingAccountPayable.BalanceAmount) - (decimal)paymentData.PaymentAmount;
                    if (existingAccountPayable.BalanceAmount == 0)
                    {
                        var transactionStatusPaid = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Paid");
                        existingAccountPayable.TransactionStatusId = transactionStatusPaid.TransactionStatusId;
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
