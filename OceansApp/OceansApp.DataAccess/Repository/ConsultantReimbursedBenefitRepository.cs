using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
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
            parameters.Add("@BenefitPaid", filtersAndPagination.Filters.BenefitPaid, DbType.Boolean);
            parameters.Add("@StartDate", filtersAndPagination.Filters.StartDate, DbType.Date);
            parameters.Add("@EndDate", filtersAndPagination.Filters.EndDate, DbType.Date);
            parameters.Add("@BenefitId", filtersAndPagination.Filters.BenefitId, DbType.Int32);

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
        public void Update(ConsultantReimbursedBenefit obj)
        {
            _db.CONSULTANT_REIMBURSED_BENEFITS.Update(obj);
        }

    }
}
