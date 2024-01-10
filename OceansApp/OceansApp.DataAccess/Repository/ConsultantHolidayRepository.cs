using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using Dapper;
using System.Text;
using System.Data;
using OceansApp.Models.ViewModels.Holidays;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantHolidayRepository : Repository<ConsultantHoliday>, IConsultantHolidayRepository
    {
        private ApplicationDbContext _db;
        public ConsultantHolidayRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<List<int>> GetHolidaysYears()
        {
            var years = await _db.CONSULTANT_HOLIDAYS
                           .GroupBy(ch => ch.Year)
                           .OrderBy(g => g.Key)
                           .Select(g => g.Key)
                           .ToListAsync();
            return years;
        }
        public async Task<(List<HolidaysGetAllWithFiltersVM> holidays, int totalCount)> GetAllHolidaysWithFiltersAsync(HolidaysPaginationFiltersVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
            parameters.Add("@Year", filtersAndPagination.Filters.Year, DbType.Int32);
            parameters.Add("@FieldToOrder", filtersAndPagination.OrderBy.FieldToOrder, DbType.String);
            parameters.Add("@DirectionOrder", filtersAndPagination.OrderBy.DirectionOrder, DbType.String);
            parameters.Add("@Skip", (filtersAndPagination.Pagination.PageIndex - 1) * filtersAndPagination.Pagination.PageSize, DbType.Int32);
            parameters.Add("@Take", filtersAndPagination.Pagination.PageSize, DbType.Int32);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var results = await connection.QueryAsync<HolidaysGetAllWithFiltersVM>("GetAllConsultantHolidaysWithFilters", parameters, commandType: CommandType.StoredProcedure);
            var totalCount = parameters.Get<int>("@TotalCount");

            var holidays = results.ToList();

            return (holidays, totalCount);
        }



        public void Update(ConsultantHoliday obj)
        {
            _db.CONSULTANT_HOLIDAYS.Update(obj);
        }

    }
}
