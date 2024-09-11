using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.PaymentBookEntries;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class PaymentBookEntryParentRepository : Repository<PaymentBookEntryParent>, IPaymentBookEntryParentRepository
    {
        private ApplicationDbContext _db;
        public PaymentBookEntryParentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<(List<BookEntriesGetAllWithFiltersVM> bookEntries, int totalCount)> GetAllBookEntriesWithFiltersAsync(BookEntriesPaginationFiltersVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", filtersAndPagination.Filters.CompanyId, DbType.String);
            parameters.Add("@TransactionStatusId", filtersAndPagination.Filters.TransactionStatusId, DbType.Int32);

            parameters.Add("@FieldToOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.FieldToOrder, DbType.String);
            parameters.Add("@DirectionOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.DirectionOrder, DbType.String);
            parameters.Add("@Skip", (filtersAndPagination.PaginationWithoutFilters.Pagination.PageIndex - 1) * filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@Take", filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var results = await connection.QueryAsync<BookEntriesGetAllWithFiltersVM>("SP_PAYMENT_BOOK_ENTRIES_CHILD_GetAllBookEntriesWithFilters", parameters, commandType: CommandType.StoredProcedure);
            var totalCount = parameters.Get<int>("@TotalCount");
            var bookEntries = results.ToList();

            return (bookEntries, totalCount);
        }

    }
}
