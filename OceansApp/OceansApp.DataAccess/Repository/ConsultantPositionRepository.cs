using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ConsultantPositions;
using OceansApp.Models.ViewModels.Interviews;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantPositionRepository : Repository<ConsultantPosition>, IConsultantPositionRepository
    {
        private ApplicationDbContext _db;
        public ConsultantPositionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<(List<ConsultantPositionsGetAllWithFiltersVM> positions, int totalCount)> GetAllConsultantPositionsWithFiltersAsync(ConsultantPositionsPaginationFiltersVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
            parameters.Add("@MovementTypeId", filtersAndPagination.Filters.MovementTypeId, DbType.Int32);
            parameters.Add("@CostCenterId", filtersAndPagination.Filters.CostCenterId, DbType.Int32);
            parameters.Add("@AccountingAccountId", filtersAndPagination.Filters.AccountingAccountId, DbType.Int32);

            parameters.Add("@FieldToOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.FieldToOrder, DbType.String);
            parameters.Add("@DirectionOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.DirectionOrder, DbType.String);
            parameters.Add("@Skip", (filtersAndPagination.PaginationWithoutFilters.Pagination.PageIndex - 1) * filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@Take", filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var results = await connection.QueryAsync<ConsultantPositionsGetAllWithFiltersVM>("SP_CONSULTANT_POSITIONS_GetAllPositionsAccountingConfigurationWithFilters", parameters, commandType: CommandType.StoredProcedure);
            var totalCount = parameters.Get<int>("@TotalCount");
            var positions = results.ToList();

            return (positions, totalCount);
        }

        public async Task<List<GetConsultantPositionConfigurationsVM>> GetCompanyMovementTypesByPositionIdAsync(int? positionId)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PositionId", positionId, DbType.Int32);

            var results = await connection.QueryAsync<GetConsultantPositionConfigurationsVM>("SP_CONSULTANT_POSITIONS_GetCompanyMovementTypeByPositionId", parameters, commandType: CommandType.StoredProcedure);

            return results.ToList();
        }

        public async Task<MethodResponse> CreatePosition(string userIdCreatedBy,
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
        public async Task<List<GetDataForSelectVM>> GetPositionsByIsAdministrative(bool isAdministrative)
        {
            IEnumerable<ConsultantPosition> positionsListFromDb = await _db.CONSULTANT_POSITIONS.Where(x => x.IsAdministrative == isAdministrative).ToListAsync();
            List<GetDataForSelectVM> positionsToReturn = new();
            foreach (var position in positionsListFromDb)
            {
                positionsToReturn.Add(new GetDataForSelectVM
                {
                    Value = position.ConsultantPositionId,
                    Text = position.Name 
                });
            }
            return positionsToReturn;
        }

        public void Update(ConsultantPosition obj)
        {
            _db.CONSULTANT_POSITIONS.Update(obj);
        }

    }
}
