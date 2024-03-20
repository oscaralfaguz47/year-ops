using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantReimbursedBenefits;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantReimbursedBenefitRepository : Repository<ConsultantReimbursedBenefit>, IConsultantReimbursedBenefitRepository
    {
        private ApplicationDbContext _db;
        public ConsultantReimbursedBenefitRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
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

        public async Task<MethodResponse> CreateBenefitReimbursement(string userIdCreatedBy, DateTime timeZone,
            CreateUpdateConsultantBenefitReimbursementVM benefitReimbursementData)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var dateToBeReimbursed = (DateTime)benefitReimbursementData.DateToBeReimbursed;
                    MethodResponse lastVerificationBalance = await VerifyBenefitReimbursementBalance((int)benefitReimbursementData.ConsultantId,
    (int)benefitReimbursementData.BenefitId, dateToBeReimbursed.Year, (decimal)benefitReimbursementData.AmountReimbursed,
    benefitReimbursementData.ReimbursedBenefitId, transaction.GetDbTransaction());
                    if (!lastVerificationBalance.Success)
                    {
                        await transaction.RollbackAsync();
                        return lastVerificationBalance;
                    }
                    var currentUser = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userIdCreatedBy);
                    var transactionStatus = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Approved");
                    if (transactionStatus == null)
                    {
                        return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The transaction status 'Approved' was not found." };
                    }
                    ConsultantReimbursedBenefit benefitReimbursementToCreate = new()
                    {
                        BenefitId = (int)benefitReimbursementData.BenefitId,
                        Detail = benefitReimbursementData.Detail,
                        ConsultantId = (int)benefitReimbursementData.ConsultantId,
                        AmountReimbursed = (decimal)benefitReimbursementData.AmountReimbursed,
                        DateToBeReimbursed = (DateTime)benefitReimbursementData.DateToBeReimbursed,
                        TransactionStatusId = transactionStatus.TransactionStatusId,
                        CreationDate = timeZone,
                        ConsultantIdCreatedBy = currentUser.ConsultantId,
                        BenefitCategoryId = (int)benefitReimbursementData.BenefitCategoryId
                    };
                    var createdBenefitReimbursement = await _db.CONSULTANT_REIMBURSED_BENEFITS.AddAsync(benefitReimbursementToCreate);
                    await _db.SaveChangesAsync();
                    if (createdBenefitReimbursement.Entity.ConsultantId > 0)
                    {
                        await transaction.CommitAsync();
                        return new MethodResponse
                        {
                            Success = true,
                            Message = $"The Benefit Reimbursement was created successfully."
                        };
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return new MethodResponse { MessageType = "Exception Error", Success = false, Message = "Something went wrong creating the benefit reimburesement, please try again." };
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
                }
            }
        }

        public async Task<MethodResponse> UpdateBenefitReimbursement(string userActionedBy, DateTime timeZone, CreateUpdateConsultantBenefitReimbursementVM benefitReimbursementData)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var dateToBeReimbursed = (DateTime)benefitReimbursementData.DateToBeReimbursed;
                    MethodResponse lastVerificationBalance = await VerifyBenefitReimbursementBalance((int)benefitReimbursementData.ConsultantId,
    (int)benefitReimbursementData.BenefitId, dateToBeReimbursed.Year, (decimal)benefitReimbursementData.AmountReimbursed,
    benefitReimbursementData.ReimbursedBenefitId, transaction.GetDbTransaction());
                    if (!lastVerificationBalance.Success)
                    {
                        await transaction.RollbackAsync();
                        return lastVerificationBalance;
                    }
                    var existingBenefitReimbursement = await _db.CONSULTANT_REIMBURSED_BENEFITS.FirstOrDefaultAsync(x => x.ReimbursedBenefitId == benefitReimbursementData.ReimbursedBenefitId);
                    if (existingBenefitReimbursement == null)
                    {
                        await transaction.RollbackAsync();
                        return new MethodResponse { MessageType = "Not Found", Success = false, Message = "The Benefit Reimbursement was not found." };
                    }

                    var currentUser = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userActionedBy);

                    existingBenefitReimbursement.BenefitId = (int)benefitReimbursementData.BenefitId;
                    existingBenefitReimbursement.BenefitCategoryId = (int)benefitReimbursementData.BenefitCategoryId;
                    existingBenefitReimbursement.Detail = benefitReimbursementData.Detail;
                    existingBenefitReimbursement.ConsultantId = (int)benefitReimbursementData.ConsultantId;
                    existingBenefitReimbursement.AmountReimbursed = (decimal)benefitReimbursementData.AmountReimbursed;
                    existingBenefitReimbursement.DateToBeReimbursed = (DateTime)benefitReimbursementData.DateToBeReimbursed;
                    existingBenefitReimbursement.LastUpdateDate = timeZone;
                    existingBenefitReimbursement.ConsultantIdLastUpdatedBy = currentUser.ConsultantId;

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new MethodResponse { Success = true, Message = $"The Consultant reimbursement was updated successfully." };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
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

        public async Task<MethodResponse> RejectBenefitReimbursement(string userActionedBy, DateTime timeZone, int benetifReimbursementId)
        {
            try
            {
                var benefitReimbursementToReject = await _db.CONSULTANT_REIMBURSED_BENEFITS.FirstOrDefaultAsync(x => x.ReimbursedBenefitId == benetifReimbursementId);
                if (benefitReimbursementToReject == null)
                {
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The Benefit Reimbursement is no longer in the database, it was removed before your request." };
                }
                var transactionRejectedStatus = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Rejected");
                if (transactionRejectedStatus == null)
                {
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The transaction 'Rejected' was not found in the database." };
                }
                var consultantUserActionedBy = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userActionedBy);
                benefitReimbursementToReject.TransactionStatusId = transactionRejectedStatus.TransactionStatusId;
                benefitReimbursementToReject.ConsultantIdLastUpdatedBy = consultantUserActionedBy.ConsultantId;
                benefitReimbursementToReject.LastUpdateDate = timeZone;

                await _db.SaveChangesAsync();
                return new MethodResponse { Success = true, Message = $"The Benefit Reimbursement was rejected successfully." };
            }
            catch (Exception ex)
            {
                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
            }
        }
        private async Task<MethodResponse> VerifyBenefitReimbursementBalance(int consultantId, int benefitId, int year, decimal amountToBeReimbursed,
        int? reimbursedBenefitIdToIgnore, IDbTransaction transaction)
        {
            GetConsumedAmountVM currentConsumedAmount = await GetConsumedAmountPerYearByConsultant(consultantId,
                   benefitId, year, amountToBeReimbursed, reimbursedBenefitIdToIgnore, transaction);
            if (!currentConsumedAmount.Applicable && currentConsumedAmount.ConsumedAmount == 0)
            {
                return new MethodResponse
                {
                    MessageType = "Validation Error",
                    Success = false,
                    Message = $"You cannot apply an amount greater than ${currentConsumedAmount.ConfiguredBenefitAmount} for the selected benefit."
                };
            }
            if (!currentConsumedAmount.Applicable && currentConsumedAmount.ConsumedAmount > 0)
            {
                var secondMessage = "";
                if (currentConsumedAmount.ConsumedAmount != currentConsumedAmount.ConfiguredBenefitAmount)
                {
                    secondMessage = $"Try with an amount of ${(currentConsumedAmount.ConfiguredBenefitAmount - currentConsumedAmount.ConsumedAmount)} or less.";
                }
                return new MethodResponse
                {
                    MessageType = "Validation Error",
                    Success = false,
                    Message = $"The consultant consumed amount is: ${currentConsumedAmount.ConsumedAmount}. The maximun amount allowed is ${currentConsumedAmount.ConfiguredBenefitAmount} for the selected benefit. {secondMessage}"
                };
            }
            return new MethodResponse
            {
                Success = true
            };
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


    }
}
