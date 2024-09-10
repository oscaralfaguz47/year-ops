using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.JournalAccountsPayable;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class JournalAccountPayableRepository : Repository<JournalAccountPayable>, IJournalAccountPayableRepository
    {
        private ApplicationDbContext _db;
        public JournalAccountPayableRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<(List<JournalAccountsPayableGetAllWithFiltersVM> journalAccountsPayable, int totalCount)> GetAllJournalAccountsPayableWithFiltersAsync(JournalAccountsPayablePaginationFiltersVM filtersAndPagination)
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

            var results = await connection.QueryAsync<JournalAccountsPayableGetAllWithFiltersVM>("SP_JOURNAL_ACCOUNTS_PAYABLE_GetAllJournalAccountsPayableWithFilters", parameters, commandType: CommandType.StoredProcedure);
            var totalCount = parameters.Get<int>("@TotalCount");
            var journalAccountsPayable = results.ToList();

            return (journalAccountsPayable, totalCount);
        }


    }
}
