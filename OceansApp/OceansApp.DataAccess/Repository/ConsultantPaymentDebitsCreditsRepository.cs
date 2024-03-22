using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
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

    }
}
