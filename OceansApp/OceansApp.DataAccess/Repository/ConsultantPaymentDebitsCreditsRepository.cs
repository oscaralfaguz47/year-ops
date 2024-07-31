using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantPaymentsDebitsCredits;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantPaymentDebitsCreditsRepository : Repository<ConsultantPaymentDebitsCredits>, IConsultantPaymentDebitsCreditsRepository
    {
        private ApplicationDbContext _db;
        public ConsultantPaymentDebitsCreditsRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<(List<ConsultantPaymentDebitsCreditsGetAllWithFiltersVM> debitsCredits, int totalCount)> GetAllPaymentsDebitsCreditsWithFiltersAsync(ConsultantPaymentsDebitsCreditsPaginationFiltersVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
            parameters.Add("@TransactionStatusId", filtersAndPagination.Filters.TransactionStatusId, DbType.Int32);
            parameters.Add("@TransactionTypeId", filtersAndPagination.Filters.TransactionTypeId, DbType.Int32);
            parameters.Add("@StartDate", filtersAndPagination.Filters.StartDate, DbType.Date);
            parameters.Add("@EndDate", filtersAndPagination.Filters.EndDate, DbType.Date);

            parameters.Add("@FieldToOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.FieldToOrder, DbType.String);
            parameters.Add("@DirectionOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.DirectionOrder, DbType.String);
            parameters.Add("@Skip", (filtersAndPagination.PaginationWithoutFilters.Pagination.PageIndex - 1) * filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@Take", filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var results = await connection.QueryAsync<ConsultantPaymentDebitsCreditsGetAllWithFiltersVM>("SP_CONSULTANT_PAYMENTS_DEBITS_CREDITS_GetAllPaymentsDebitsCreditsWithFilters", parameters, commandType: CommandType.StoredProcedure);
            var totalCount = parameters.Get<int>("@TotalCount");
            var paymentDebitsCredits = results.ToList();

            return (paymentDebitsCredits, totalCount);
        }

        public async Task<MethodResponse> CreateDebitCredit(string userIdCreatedBy,
            CreateUpdateConsultantPaymentDebitCreditVM debitCreditData)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var currentUser = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userIdCreatedBy);
                    var transactionStatus = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Approved");
                    if (transactionStatus == null)
                    {
                        return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The transaction status 'Approved' was not found." };
                    }
                    var transactionType = await _db.TRANSACTION_TYPES.FirstOrDefaultAsync(x => x.Name == debitCreditData.TransactionTypeName);
                    if (transactionType == null)
                    {
                        return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The transaction type '{debitCreditData.TransactionTypeName}' was not found." };
                    }
                    ConsultantPaymentDebitsCredits debitCreditToCreate = new()
                    {
                        AccountingAccountId = (int)debitCreditData.AccountingAccountId,
                        CostCenterId = (int)debitCreditData.CostCenterId,
                        Detail = debitCreditData.Detail,
                        ConsultantId = (int)debitCreditData.ConsultantId,
                        Quantity = (decimal)debitCreditData.Quantity,
                        Amount = (decimal)debitCreditData.Amount,
                        ActionDateWithinFortnight = (DateTime)debitCreditData.ActionDateWithinFortnight,
                        TransactionStatusId = transactionStatus.TransactionStatusId,
                        CreationDate = DateTime.UtcNow,
                        ConsultantIdCreatedBy = currentUser.ConsultantId,
                        TransactionTypeId = transactionType.TransactionTypeId
                    };
                    var createdDebitCredit = await _db.CONSULTANT_PAYMENTS_DEBITS_CREDITS.AddAsync(debitCreditToCreate);
                    await _db.SaveChangesAsync();
                    if (createdDebitCredit.Entity.ConsultantPaymentDebitsCreditsId > 0)
                    {
                        await transaction.CommitAsync();
                        return new MethodResponse
                        {
                            Success = true,
                            Message = $"The {debitCreditData.TransactionTypeName} was created successfully."
                        };
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"Something went wrong creating the {debitCreditData.TransactionTypeName}, please try again." };
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
                }
            }
        }

        public async Task<MethodResponse> UpdateDebitCredit(string userActionedBy, CreateUpdateConsultantPaymentDebitCreditVM debitCreditData)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingDebitCredit = await _db.CONSULTANT_PAYMENTS_DEBITS_CREDITS.FirstOrDefaultAsync(x => x.ConsultantPaymentDebitsCreditsId == debitCreditData.ConsultantPaymentDebitsCreditsId);
                    if (existingDebitCredit == null)
                    {
                        await transaction.RollbackAsync();
                        return new MethodResponse { MessageType = "Not Found", Success = false, Message = $"The debit/credit was not found." };
                    }
                    var transactionType = await _db.TRANSACTION_TYPES.FirstOrDefaultAsync(x => x.Name == debitCreditData.TransactionTypeName);
                    if (transactionType == null)
                    {
                        return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The transaction type '{debitCreditData.TransactionTypeName}' was not found." };
                    }
                    var currentUser = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userActionedBy);

                    existingDebitCredit.AccountingAccountId = (int)debitCreditData.AccountingAccountId;
                    existingDebitCredit.CostCenterId = (int)debitCreditData.CostCenterId;
                    existingDebitCredit.Detail = debitCreditData.Detail;
                    existingDebitCredit.ConsultantId = (int)debitCreditData.ConsultantId;
                    existingDebitCredit.Quantity = (decimal)debitCreditData.Quantity;
                    existingDebitCredit.Amount = (decimal)debitCreditData.Amount;
                    existingDebitCredit.ActionDateWithinFortnight = (DateTime)debitCreditData.ActionDateWithinFortnight;
                    existingDebitCredit.TransactionTypeId = transactionType.TransactionTypeId;
                    existingDebitCredit.LastUpdateDate = DateTime.UtcNow;
                    existingDebitCredit.ConsultantIdLastUpdatedBy = currentUser.ConsultantId;

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new MethodResponse { Success = true, Message = $"The Consultant transaction was updated successfully." };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
                }
            }
        }

        public async Task<CreateUpdateConsultantPaymentDebitCreditVM> GetDebitCreditDataById(int consultantPaymentDebitsCreditsId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ConsultantPaymentDebitsCreditsId", consultantPaymentDebitsCreditsId);

            using (var multiResultSet = await connection.QueryMultipleAsync("SP_CONSULTANT_PAYMENTS_DEBITS_CREDITS_GetDebitCreditDataById", parameters, commandType: CommandType.StoredProcedure))
            {
                var debitCredit = await multiResultSet.ReadFirstOrDefaultAsync<CreateUpdateConsultantPaymentDebitCreditVM>();
                if (debitCredit != null)
                {
                    return debitCredit;
                }
                else
                {
                    return null;
                }

            }
        }

        public async Task<MethodResponse> RejectDebitCredit(string userActionedBy, int consultantPaymentDebitsCreditsId)
        {
            try
            {
                var debitCreditToReject = await _db.CONSULTANT_PAYMENTS_DEBITS_CREDITS.FirstOrDefaultAsync(x => x.ConsultantPaymentDebitsCreditsId == consultantPaymentDebitsCreditsId);
                if (debitCreditToReject == null)
                {
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The Debit/Credit is no longer in the database, it was removed before your request." };
                }
                var transactionRejectedStatus = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Rejected");
                if (transactionRejectedStatus == null)
                {
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The transaction 'Rejected' was not found in the database." };
                }
                var consultantUserActionedBy = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userActionedBy);
                debitCreditToReject.TransactionStatusId = transactionRejectedStatus.TransactionStatusId;
                debitCreditToReject.ConsultantIdLastUpdatedBy = consultantUserActionedBy.ConsultantId;
                debitCreditToReject.LastUpdateDate = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return new MethodResponse { Success = true, Message = $"The transaction was rejected successfully." };
            }
            catch (Exception ex)
            {
                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
            }
        }

        public async Task<List<GetApprovedDebitsCreditsWhereConsultantVM>> GetApprovedDebitsCreditsWhereConsultantInThePeriod(int consultantId,
          DateTime startDate, DateTime endDate)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ConsultantId", consultantId);
            parameters.Add("@StartDate", startDate);
            parameters.Add("@EndDate", endDate);

            var results = await connection.QueryAsync<GetApprovedDebitsCreditsWhereConsultantVM>("SP_CONSULTANT_PAYMENTS_DEBITS_CREDITS_GetApprovedDebitCreditWhereConsultantInThePeriod", parameters, commandType: CommandType.StoredProcedure);
            return results.ToList();
        }

    }
}
