using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Interviews;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class InterviewRepository : Repository<Interview>, IInterviewRepository
    {
        private ApplicationDbContext _db;
        public InterviewRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<(List<InterviewsGetAllWithFiltersVM> interviews, int totalCount)> GetAllInterviewsWithFiltersAsync(InterviewsPaginationFiltersVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
            parameters.Add("@StartDate", filtersAndPagination.Filters.StartDate, DbType.Date);
            parameters.Add("@EndDate", filtersAndPagination.Filters.EndDate, DbType.Date);
            parameters.Add("@TransactionStatusId", filtersAndPagination.Filters.TransactionStatusId, DbType.Int32);

            parameters.Add("@FieldToOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.FieldToOrder, DbType.String);
            parameters.Add("@DirectionOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.DirectionOrder, DbType.String);
            parameters.Add("@Skip", (filtersAndPagination.PaginationWithoutFilters.Pagination.PageIndex - 1) * filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@Take", filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var results = await connection.QueryAsync<InterviewsGetAllWithFiltersVM>("SP_INTERVIEWS_GetAllInterviewsWithFilters", parameters, commandType: CommandType.StoredProcedure);
            var totalCount = parameters.Get<int>("@TotalCount");
            var interviews = results.ToList();

            return (interviews, totalCount);
        }

        public async Task<MethodResponse> CreateInterview(string userIdCreatedBy,
            CreateUpdateInterviewVM interviewData)
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
                    Interview interviewToCreate = new()
                    {
                        ConsultantId = (int)interviewData.ConsultantId,
                        DurationMinutes = (decimal)interviewData.DurationMinutes,
                        Date = (DateTime)interviewData.Date,
                        TransactionStatusId = transactionStatus.TransactionStatusId,
                        CreationDate = DateTime.UtcNow,
                        ConsultantIdCreatedBy = currentUser.ConsultantId
                    };
                    var createdInterview = await _db.INTERVIEWS.AddAsync(interviewToCreate);
                    await _db.SaveChangesAsync();
                    if (createdInterview.Entity.InterviewId > 0)
                    {
                        await transaction.CommitAsync();
                        return new MethodResponse
                        {
                            Success = true,
                            Message = $"The Interview was created successfully."
                        };
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"Something went wrong creating the Interview, please try again." };
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
                }
            }
        }

        public async Task<MethodResponse> UpdateInterview(string userActionedBy, CreateUpdateInterviewVM interviewData)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingInterview = await _db.INTERVIEWS.FirstOrDefaultAsync(x => x.InterviewId == interviewData.InterviewId);
                    if (existingInterview == null)
                    {
                        await transaction.RollbackAsync();
                        return new MethodResponse { MessageType = "Not Found", Success = false, Message = $"The Interview was not found." };
                    }
                    var currentUser = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userActionedBy);

                    existingInterview.ConsultantId = (int)interviewData.ConsultantId;
                    existingInterview.DurationMinutes = (decimal)interviewData.DurationMinutes;
                    existingInterview.Date = (DateTime)interviewData.Date;
                    existingInterview.LastUpdateDate = DateTime.UtcNow;
                    existingInterview.ConsultantIdLastUpdatedBy = currentUser.ConsultantId;

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new MethodResponse { Success = true, Message = $"The Interview was updated successfully." };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
                }
            }
        }

        public async Task<CreateUpdateInterviewVM> GetInterviewDataById(int interviewId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@InterviewId", interviewId);

            using (var multiResultSet = await connection.QueryMultipleAsync("SP_INTERVIEWS_GetInterviewDataById", parameters, commandType: CommandType.StoredProcedure))
            {
                var interview = await multiResultSet.ReadFirstOrDefaultAsync<CreateUpdateInterviewVM>();
                if (interview != null)
                {
                    return interview;
                }
                else
                {
                    return null;
                }

            }
        }

        public async Task<MethodResponse> RejectInterview(string userActionedBy, int interviewId)
        {
            try
            {
                var interviewToReject = await _db.INTERVIEWS.FirstOrDefaultAsync(x => x.InterviewId == interviewId);
                if (interviewToReject == null)
                {
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The Interview is no longer in the database, it was removed before your request." };
                }
                var transactionRejectedStatus = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Rejected");
                if (transactionRejectedStatus == null)
                {
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The transaction 'Rejected' was not found in the database." };
                }
                var consultantUserActionedBy = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userActionedBy);
                interviewToReject.TransactionStatusId = transactionRejectedStatus.TransactionStatusId;
                interviewToReject.ConsultantIdLastUpdatedBy = consultantUserActionedBy.ConsultantId;
                interviewToReject.LastUpdateDate = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return new MethodResponse { Success = true, Message = $"The interview was rejected successfully." };
            }
            catch (Exception ex)
            {
                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
            }
        }

        public async Task<List<GetApprovedInterviewsWhereConsultantVM>> GetApprovedInterviewsWhereConsultantInThePeriod(int consultantId,
          DateTime startDate, DateTime endDate)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ConsultantId", consultantId);
            parameters.Add("@StartDate", startDate);
            parameters.Add("@EndDate", endDate);

            var results = await connection.QueryAsync<GetApprovedInterviewsWhereConsultantVM>("SP_INTERVIEWS_GetApprovedInterviewsWhereConsultantInThePeriod", parameters, commandType: CommandType.StoredProcedure);
            return results.ToList();
        }

    }
}
