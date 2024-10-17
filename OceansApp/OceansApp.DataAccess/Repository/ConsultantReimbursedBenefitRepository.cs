using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantReimbursedBenefits;
using OceansApp.Models.ViewModels.Dashboard;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantReimbursedBenefitRepository : Repository<ConsultantReimbursedBenefit>, IConsultantReimbursedBenefitRepository
    {
        private ApplicationDbContext _db;
        private readonly IConsultantAndBenefitRepository _consultantAndConsultantRepository;
        private readonly IConsultantPaymentRepository _consultantPaymentRepository;
        private readonly IApplicationRoleClaimRepository _applicationRoleClaimRepository;
        public ConsultantReimbursedBenefitRepository(ApplicationDbContext db, IUnitOfWork unitOfWork) : base(db)
        {
            _db = db;
            _consultantAndConsultantRepository = unitOfWork.ConsultantAndBenefit;
            _consultantPaymentRepository = unitOfWork.ConsultantPayment;
            _applicationRoleClaimRepository = unitOfWork.ApplicationRoleClaim;
        }

        public async Task<(List<ConsultantReimbursedBenefitsGetAllWithFiltersVM> reimbursedBenefits, int totalCount)> GetAllConsultantsReimbursedBenefitsWithFiltersAsync(ConsultantReimbursedBenefitsPaginationFiltersVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
            parameters.Add("@TransactionStatusId", filtersAndPagination.Filters.TransactionStatusId, DbType.Int32);
            parameters.Add("@StartDate", filtersAndPagination.Filters.StartDate, DbType.Date);
            parameters.Add("@EndDate", filtersAndPagination.Filters.EndDate, DbType.Date);
            parameters.Add("@BenefitId", filtersAndPagination.Filters.BenefitId, DbType.Int32);
            parameters.Add("@BenefitCategoryId", filtersAndPagination.Filters.BenefitCategoryId, DbType.Int32);

            parameters.Add("@FieldToOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.FieldToOrder, DbType.String);
            parameters.Add("@DirectionOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.DirectionOrder, DbType.String);
            parameters.Add("@Skip", (filtersAndPagination.PaginationWithoutFilters.Pagination.PageIndex - 1) * filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@Take", filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var results = await connection.QueryAsync<ConsultantReimbursedBenefitsGetAllWithFiltersVM>("SP_CONSULTANT_REIMBURSED_BENEFITS_GetAllConsultantReimbursedBenefitsWithFilters", parameters, commandType: CommandType.StoredProcedure);
            var totalCount = parameters.Get<int>("@TotalCount");
            var reimbursedBenefits = results.ToList();

            return (reimbursedBenefits, totalCount);
        }

        public async Task<MethodResponse> CreateBenefitReimbursement(string userIdCreatedBy,
            CreateUpdateConsultantBenefitReimbursementVM benefitReimbursementData)
        {
            bool isAuthorizedToCreateInPaidPeriod = await _applicationRoleClaimRepository.ValidateRoleClaimAsync(userIdCreatedBy, "BasicPaymentSheets", "Have access to manage the basics of payment sheets");

            bool existsPayment = await _consultantPaymentRepository
                .ValidateConsultantPaymentByDateAsync((DateTime)benefitReimbursementData.DateToBeReimbursed,
                (int)benefitReimbursementData.ConsultantId);

            if (existsPayment && !isAuthorizedToCreateInPaidPeriod) return MethodResponse
                    .CreateFailureValidationResponse($"The action date: '{benefitReimbursementData.DateToBeReimbursed.Value.ToString("MM/dd/yyyy")}' is not allowed, the consultant already has a payment for that period.");

            var currentUser = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userIdCreatedBy);
            if (currentUser == null) return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The consultant was not found." };

            var approvedStatus = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Approved");
            if (approvedStatus == null) return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The transaction status 'Approved' was not found." };

            var benefit = await _db.CONSULTANT_BENEFITS.FirstOrDefaultAsync(x => x.BenefitId == benefitReimbursementData.BenefitId);
            if (benefit == null) return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The benefit was not found." };

            using (var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var existingConsultantAndBenefit = await _consultantAndConsultantRepository
                .CreateConsultantAndBenefitIfNotExists((int)benefitReimbursementData.ConsultantId, benefit);

                    if (benefitReimbursementData.AmountReimbursed > existingConsultantAndBenefit.BalanceAmount || benefitReimbursementData.AmountReimbursed > benefit.Amount)
                    {
                        string validationMessage = $"You cannot apply an amount greater than ${benefit.Amount} for the seleted benefit. The current balance for " +
                            $"the selected consultant is: ${existingConsultantAndBenefit.BalanceAmount}";

                        if (currentUser.ConsultantId == benefitReimbursementData.ConsultantId)
                        {
                            validationMessage = $"You cannot apply an amount greater than ${benefit.Amount} for the seleted benefit. Your current balance " +
                            $"is: ${existingConsultantAndBenefit.BalanceAmount}";
                        }
                        await transaction.RollbackAsync();
                        return MethodResponse.CreateFailureValidationResponse(validationMessage);
                    }

                    ConsultantReimbursedBenefit benefitReimbursementToCreate = new()
                    {
                        BenefitId = (int)benefitReimbursementData.BenefitId,
                        Detail = benefitReimbursementData.Detail,
                        ConsultantId = (int)benefitReimbursementData.ConsultantId,
                        AmountReimbursed = (decimal)benefitReimbursementData.AmountReimbursed,
                        DateToBeReimbursed = (DateTime)benefitReimbursementData.DateToBeReimbursed,
                        TransactionStatusId = approvedStatus.TransactionStatusId,
                        CreationDate = DateTime.UtcNow,
                        ConsultantIdCreatedBy = currentUser.ConsultantId,
                        BenefitCategoryId = (int)benefitReimbursementData.BenefitCategoryId
                    };

                    await _db.CONSULTANT_REIMBURSED_BENEFITS.AddAsync(benefitReimbursementToCreate);
                    await _db.SaveChangesAsync();

                    ConsultantAndBenefitHistory historyToCreate = new()
                    {
                        CreationDate = DateTime.UtcNow,
                        UserCreatedById = userIdCreatedBy,
                        ConsultantAndBenefitId = existingConsultantAndBenefit.Id,
                        OldValue = existingConsultantAndBenefit.BalanceAmount,
                        NewValue = (existingConsultantAndBenefit.BalanceAmount - (decimal)benefitReimbursementData.AmountReimbursed),
                        ReimbursedBenefitId = benefitReimbursementToCreate.ReimbursedBenefitId
                    };
                    await _db.CONSULTANTS_AND_BENEFITS_HISTORY.AddAsync(historyToCreate);
                    await _db.SaveChangesAsync();

                    existingConsultantAndBenefit.BalanceAmount -= (decimal)benefitReimbursementData.AmountReimbursed;
                    await _db.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return new MethodResponse
                    {
                        Success = true,
                        Message = $"The Benefit Reimbursement was created successfully."
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
                }
            }
        }

        public async Task<MethodResponse> UpdateBenefitReimbursement(string userActionedBy,
   CreateUpdateConsultantBenefitReimbursementVM benefitReimbursementData)
        {
            bool existsPayment = await _consultantPaymentRepository
                .ValidateConsultantPaymentByDateAsync((DateTime)benefitReimbursementData.DateToBeReimbursed,
                (int)benefitReimbursementData.ConsultantId);

            if (existsPayment) return MethodResponse
                    .CreateFailureValidationResponse($"The action date: '{benefitReimbursementData.DateToBeReimbursed.Value.ToString("MM/dd/yyyy")}' is not allowed, the consultant already has a payment for that period.");

            var currentUser = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userActionedBy);
            if (currentUser == null)
                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The consultant was not found." };

            var benefit = await _db.CONSULTANT_BENEFITS.FirstOrDefaultAsync(x => x.BenefitId == benefitReimbursementData.BenefitId);
            if (benefit == null)
                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The benefit was not found." };

            using (var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var existingBenefitReimbursement = await _db.CONSULTANT_REIMBURSED_BENEFITS
                        .FirstOrDefaultAsync(x => x.ReimbursedBenefitId == benefitReimbursementData.ReimbursedBenefitId);
                    if (existingBenefitReimbursement == null)
                        return new MethodResponse { MessageType = "Not Found", Success = false, Message = "The Benefit Reimbursement was not found." };

                    // Check for changes in benefit details
                    bool isBenefitChanged = existingBenefitReimbursement.BenefitId != benefitReimbursementData.BenefitId;
                    bool isConsultantChanged = existingBenefitReimbursement.ConsultantId != benefitReimbursementData.ConsultantId;
                    bool isAmountReimbursedChanged = Math.Round(existingBenefitReimbursement.AmountReimbursed, 2) != Math.Round((decimal)benefitReimbursementData.AmountReimbursed, 2);

                    if (!isAmountReimbursedChanged &&
                        existingBenefitReimbursement.ConsultantId == benefitReimbursementData.ConsultantId &&
                        existingBenefitReimbursement.BenefitId == benefitReimbursementData.BenefitId &&
                        existingBenefitReimbursement.BenefitCategoryId == benefitReimbursementData.BenefitCategoryId &&
                        existingBenefitReimbursement.DateToBeReimbursed == benefitReimbursementData.DateToBeReimbursed &&
                        existingBenefitReimbursement.Detail == benefitReimbursementData.Detail)
                    {
                        return new MethodResponse { Success = true, Message = $"No changes were detected." };
                    }

                    // Fetch existing benefit associated with the consultant (OLD Consultant)
                    var consultantAndBenefitExists = await _db.CONSULTANTS_AND_BENEFITS
                        .FirstOrDefaultAsync(x => x.ConsultantId == existingBenefitReimbursement.ConsultantId &&
                        x.BenefitId == existingBenefitReimbursement.BenefitId);

                    // Fetch benefit associated with the new Consultant and Benefit (NEW Consultant and Benefit)
                    var consultantAndBenefitEditedExists = await _db.CONSULTANTS_AND_BENEFITS
                        .FirstOrDefaultAsync(x => x.ConsultantId == benefitReimbursementData.ConsultantId &&
                        x.BenefitId == benefit.BenefitId);

                    // --- VALIDATIONS ----

                    // Scenario 1: If the refunded amount is greater than the new benefit limit, block it
                    if (benefitReimbursementData.AmountReimbursed > benefit.Amount)
                    {
                        string validationMessage = $"You cannot apply an amount greater than ${benefit.Amount} for the selected benefit.";

                        // Show balance if record exists
                        if (consultantAndBenefitEditedExists != null)
                        {
                            validationMessage += $" The current balance for the selected consultant is: ${consultantAndBenefitEditedExists.BalanceAmount}";
                        }
                        await transaction.RollbackAsync();
                        return MethodResponse.CreateFailureValidationResponse(validationMessage);
                    }

                    // Scenario 2: Validate against current balance (for consultant or benefit changes)
                    if (isConsultantChanged || isBenefitChanged)
                    {
                        if (consultantAndBenefitEditedExists != null)
                        {
                            if (benefitReimbursementData.AmountReimbursed > consultantAndBenefitEditedExists.BalanceAmount)
                            {
                                string validationMessage = $"You cannot apply an amount greater than ${benefit.Amount} for the selected benefit. The current balance for " +
                                                           $"the selected consultant is: ${consultantAndBenefitEditedExists.BalanceAmount}";
                                await transaction.RollbackAsync();
                                return MethodResponse.CreateFailureValidationResponse(validationMessage);
                            }
                        }
                        else
                        {
                            if (benefitReimbursementData.AmountReimbursed > benefit.Amount)
                            {
                                string validationMessage = $"You cannot apply an amount greater than ${benefit.Amount} for the selected benefit.";
                                await transaction.RollbackAsync();
                                return MethodResponse.CreateFailureValidationResponse(validationMessage);
                            }
                        }
                    }

                    // Scenario 3: Validate against current balance (for the same consultant and same benefit)
                    if (!isConsultantChanged && !isBenefitChanged && consultantAndBenefitExists != null)
                    {
                        if (benefitReimbursementData.AmountReimbursed > consultantAndBenefitExists.BalanceAmount)
                        {
                            string validationMessage = $"You cannot apply an amount greater than ${benefit.Amount} for the selected benefit. The current balance for " +
                                                       $"the selected consultant is: ${consultantAndBenefitExists.BalanceAmount}";
                            await transaction.RollbackAsync();
                            return MethodResponse.CreateFailureValidationResponse(validationMessage);
                        }
                    }

                    // Handle ConsultantId and BenefitId changes and adjust balances
                    if (isConsultantChanged || isBenefitChanged)
                    {
                        if (consultantAndBenefitExists != null)
                        {
                            ConsultantAndBenefitHistory historyForOldConsultant = new()
                            {
                                CreationDate = DateTime.UtcNow,
                                UserCreatedById = userActionedBy,
                                ConsultantAndBenefitId = consultantAndBenefitExists.Id,
                                OldValue = consultantAndBenefitExists.BalanceAmount,
                                NewValue = consultantAndBenefitExists.BalanceAmount + existingBenefitReimbursement.AmountReimbursed,
                                ReimbursedBenefitId = existingBenefitReimbursement.ReimbursedBenefitId
                            };
                            await _db.CONSULTANTS_AND_BENEFITS_HISTORY.AddAsync(historyForOldConsultant);
                            await _db.SaveChangesAsync();

                            // Adjust the balance for the old consultant and benefit
                            consultantAndBenefitExists.BalanceAmount += existingBenefitReimbursement.AmountReimbursed;
                            await _db.SaveChangesAsync();
                        }

                        if (consultantAndBenefitEditedExists == null)
                        {
                            var newConsultantAndBenefit = await _consultantAndConsultantRepository
                                .CreateConsultantAndBenefitIfNotExists((int)benefitReimbursementData.ConsultantId, benefit);

                            ConsultantAndBenefitHistory historyForNewConsultant = new()
                            {
                                CreationDate = DateTime.UtcNow,
                                UserCreatedById = userActionedBy,
                                ConsultantAndBenefitId = newConsultantAndBenefit.Id,
                                OldValue = newConsultantAndBenefit.BalanceAmount,
                                NewValue = newConsultantAndBenefit.BalanceAmount - (decimal)benefitReimbursementData.AmountReimbursed,
                                ReimbursedBenefitId = existingBenefitReimbursement.ReimbursedBenefitId
                            };
                            await _db.CONSULTANTS_AND_BENEFITS_HISTORY.AddAsync(historyForNewConsultant);
                            await _db.SaveChangesAsync();

                            // Adjust the balance for the new consultant and new benefit
                            newConsultantAndBenefit.BalanceAmount -= (decimal)benefitReimbursementData.AmountReimbursed;
                            await _db.SaveChangesAsync();
                        }
                        else
                        {
                            ConsultantAndBenefitHistory historyForEditedConsultant = new()
                            {
                                CreationDate = DateTime.UtcNow,
                                UserCreatedById = userActionedBy,
                                ConsultantAndBenefitId = consultantAndBenefitEditedExists.Id,
                                OldValue = consultantAndBenefitEditedExists.BalanceAmount,
                                NewValue = consultantAndBenefitEditedExists.BalanceAmount - (decimal)benefitReimbursementData.AmountReimbursed,
                                ReimbursedBenefitId = existingBenefitReimbursement.ReimbursedBenefitId
                            };
                            await _db.CONSULTANTS_AND_BENEFITS_HISTORY.AddAsync(historyForEditedConsultant);
                            await _db.SaveChangesAsync();

                            consultantAndBenefitEditedExists.BalanceAmount -= (decimal)benefitReimbursementData.AmountReimbursed;
                            await _db.SaveChangesAsync();
                        }
                    }

                    // Update existing benefit reimbursement details
                    existingBenefitReimbursement.BenefitId = (int)benefitReimbursementData.BenefitId;
                    existingBenefitReimbursement.BenefitCategoryId = (int)benefitReimbursementData.BenefitCategoryId;
                    existingBenefitReimbursement.Detail = benefitReimbursementData.Detail;
                    existingBenefitReimbursement.ConsultantId = (int)benefitReimbursementData.ConsultantId;
                    existingBenefitReimbursement.AmountReimbursed = (decimal)benefitReimbursementData.AmountReimbursed;
                    existingBenefitReimbursement.DateToBeReimbursed = (DateTime)benefitReimbursementData.DateToBeReimbursed;
                    existingBenefitReimbursement.LastUpdateDate = DateTime.UtcNow;
                    existingBenefitReimbursement.ConsultantIdLastUpdatedBy = currentUser.ConsultantId;
                    await _db.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return MethodResponse.CreateSuccessResponse("The Consultant reimbursement was updated successfully");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                }
            }
        }

        public async Task<CreateUpdateConsultantBenefitReimbursementVM> GetBenefitReimbursementDataById(int benefitReimbursementId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ReimbursedBenefitId", benefitReimbursementId);

            using (var multiResultSet = await connection.QueryMultipleAsync("SP_CONSULTANT_REIMBURSED_BENEFITS_GetReimbursementDataById", parameters, commandType: CommandType.StoredProcedure))
            {
                var benefitReimbursement = await multiResultSet.ReadFirstOrDefaultAsync<CreateUpdateConsultantBenefitReimbursementVM>();
                if (benefitReimbursement != null)
                {
                    return benefitReimbursement;
                }
                else
                {
                    return null;
                }

            }
        }

        public async Task<MethodResponse> RejectBenefitReimbursement(string userActionedBy, int benetifReimbursementId)
        {
            var transactionRejectedStatus = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Rejected");
            if (transactionRejectedStatus == null)
                return MethodResponse.CreateFailureNotFoundResponse("The transaction 'Rejected' was not found in the database");

            var consultantUserActionedBy = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userActionedBy);
            if (consultantUserActionedBy == null)
                return MethodResponse.CreateFailureNotFoundResponse("The consultant was not found");

            using (var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var benefitReimbursementToReject = await _db.CONSULTANT_REIMBURSED_BENEFITS.FirstOrDefaultAsync(x => x.ReimbursedBenefitId == benetifReimbursementId);
                    if (benefitReimbursementToReject == null)
                    {
                        await transaction.RollbackAsync();
                        return MethodResponse.CreateFailureNotFoundResponse("The Benefit Reimbursement is no longer in the database, it was removed before your request");
                    }

                    benefitReimbursementToReject.TransactionStatusId = transactionRejectedStatus.TransactionStatusId;
                    benefitReimbursementToReject.ConsultantIdLastUpdatedBy = consultantUserActionedBy.ConsultantId;
                    benefitReimbursementToReject.LastUpdateDate = DateTime.UtcNow;

                    await _db.SaveChangesAsync();

                    var consultantAndBenefitExists = await _db.CONSULTANTS_AND_BENEFITS
                        .FirstOrDefaultAsync(x => x.ConsultantId == benefitReimbursementToReject.ConsultantId &&
                        x.BenefitId == benefitReimbursementToReject.BenefitId);

                    ConsultantAndBenefitHistory historyToCreate = new()
                    {
                        CreationDate = DateTime.UtcNow,
                        UserCreatedById = userActionedBy,
                        ConsultantAndBenefitId = consultantAndBenefitExists.Id,
                        OldValue = consultantAndBenefitExists.BalanceAmount,
                        NewValue = consultantAndBenefitExists.BalanceAmount + benefitReimbursementToReject.AmountReimbursed,
                        ReimbursedBenefitId = benefitReimbursementToReject.ReimbursedBenefitId
                    };
                    await _db.CONSULTANTS_AND_BENEFITS_HISTORY.AddAsync(historyToCreate);
                    await _db.SaveChangesAsync();

                    consultantAndBenefitExists.BalanceAmount += benefitReimbursementToReject.AmountReimbursed;
                    await _db.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return MethodResponse.CreateSuccessResponse("The Benefit Reimbursement was rejected successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                }
            }
        }

        public async Task<GetConsumedAmountVM> GetConsumedAmountPerYearByConsultant(
     int consultantId,
     int benefitId,
     int year,
     decimal amountToBeReimbursed,
     int? reimbursedBenefitIdToIgnore,
     IDbTransaction transaction)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ConsultantId", consultantId);
            parameters.Add("@BenefitId", benefitId);
            parameters.Add("@Year", year);
            parameters.Add("@AmountToBeReimbursed", amountToBeReimbursed);
            parameters.Add("@ReimbursedBenefitIdToIgnore", reimbursedBenefitIdToIgnore);

            using (var multiResultSet = await connection.QueryMultipleAsync(
                "SP_CONSULTANT_REIMBURSED_BENEFITS_GetConsumedAmountByConsultant",
                parameters,
                commandType: CommandType.StoredProcedure,
                transaction: transaction)) // Pasa la transacción aquí
            {
                var consumedAmount = await multiResultSet.ReadFirstOrDefaultAsync<GetConsumedAmountVM>();
                return consumedAmount ?? new GetConsumedAmountVM { ConsumedAmount = 0, Applicable = true };
            }
        }

        public async Task<List<GetApprovedBenefitsWhereConsultant>> GetApprovedBenefitsWhereConsultantInThePeriod(int consultantId,
          DateTime startDate, DateTime endDate)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ConsultantId", consultantId);
            parameters.Add("@StartDate", startDate);
            parameters.Add("@EndDate", endDate);

            var results = await connection.QueryAsync<GetApprovedBenefitsWhereConsultant>("SP_CONSULTANT_REIMBURSED_BENEFITS_GetApprovedBenefitsWhereConsultantInThePeriod", parameters, commandType: CommandType.StoredProcedure);
            return results.ToList();
        }


        public async Task<List<BenefitLastRequestsVM>> GetLastBenefitRequests(int consultantId, string benefitName)
        {
            var result = await _db.CONSULTANT_REIMBURSED_BENEFITS
                         .Where(crb => crb.ConsultantId == consultantId
                                    && crb.TransactionStatus.Name != "Rejected"
                                    && crb.ConsultantBenefit.Name == benefitName)
                         .OrderByDescending(crb => crb.CreationDate)
                         .Take(2)
                         .Select(crb => new BenefitLastRequestsVM
                         {
                             Amount = crb.AmountReimbursed,
                             Date = crb.CreationDate,
                             Status = crb.TransactionStatus.Name
                         })
                         .ToListAsync();
            return result;
        }
    }
}
