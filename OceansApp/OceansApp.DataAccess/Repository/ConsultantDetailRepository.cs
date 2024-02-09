using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Consultants;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantDetailRepository : Repository<ConsultantDetail>, IConsultantDetailRepository
    {
        private ApplicationDbContext _db;
        public ConsultantDetailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<(List<ConsultantsGetAllWithFiltersVM> consultants, int totalCount)> GetAllConsultantsWithFiltersAsync(ConsultantsPaginationFiltersVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
            parameters.Add("@CountryId", filtersAndPagination.Filters.CountryId, DbType.Int32);
            parameters.Add("@IsTwoFactorEnabled", filtersAndPagination.Filters.IsTwoFactorEnabled, DbType.Boolean);
            parameters.Add("@EmailConfirmed", filtersAndPagination.Filters.EmailConfirmed, DbType.Boolean);
            parameters.Add("@IsActive", filtersAndPagination.Filters.IsActive, DbType.Boolean);
            parameters.Add("@UserCategoryId", filtersAndPagination.Filters.UserCategoryId, DbType.Int32);

            parameters.Add("@FieldToOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.FieldToOrder, DbType.String);
            parameters.Add("@DirectionOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.DirectionOrder, DbType.String);
            parameters.Add("@Skip", (filtersAndPagination.PaginationWithoutFilters.Pagination.PageIndex - 1) * filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@Take", filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var results = await connection.QueryAsync<ConsultantsGetAllWithFiltersVM>("SP_CONSULTANT_DETAILS_GetAllConsultantsWithFilters", parameters, commandType: CommandType.StoredProcedure);
            var totalCount = parameters.Get<int>("@TotalCount");
            var consultants = results.ToList();

            return (consultants, totalCount);
        }
        public async Task<List<GetUsersSelectVM>> GetUsersByCategoryAndPositionForSelect(string userCategory, string userPosition)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@UserCategory", userCategory, DbType.String);
            parameters.Add("@UserPosition", userPosition, DbType.String);

            var results = await connection.QueryAsync<GetUsersSelectVM>("GetUsersByCategoryAndPosition", parameters, commandType: CommandType.StoredProcedure);

            var users = results.ToList();

            return (users);
        }
        public async Task<int> GetNumOfUsersByCategoryConsultantIdAndPosition(string userCategory, string userPosition, int consultantId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserCategory", userCategory, DbType.String);
            parameters.Add("@UserPosition", userPosition, DbType.String);
            parameters.Add("@ConsultantId", consultantId, DbType.Int32);

            var result = await connection.ExecuteScalarAsync<int>("GetNumOfUsersByCategoryConsultantIdAndPosition", parameters, commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<List<GetConsultantsBySearchTextVM>> GetConsultantsBySearchText(string searchText)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", searchText, DbType.String);

            var result = await connection.QueryAsync<GetConsultantsBySearchTextVM>("SP_CONSULTANT_DETAILS_SearchConsultantsBySearchText", parameters, commandType: CommandType.StoredProcedure);
            return result.ToList();
        }

        public void Update(ConsultantDetail obj)
        {
            _db.CONSULTANT_DETAILS.Update(obj);
        }

    }
}
