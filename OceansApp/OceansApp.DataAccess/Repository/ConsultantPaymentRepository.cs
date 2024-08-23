using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
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

    }
}
