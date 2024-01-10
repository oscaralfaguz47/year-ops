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

            var queryBuilder = new StringBuilder();
            var parameters = new DynamicParameters();

            queryBuilder.AppendLine(@"
                   SELECT 
                    CH.ConsultantHolidayId,
                    CH.Year,
                    CH.Name,
                    CH.CreationDate,
                    U.Name AS CreatedByName
                    FROM 
                    CONSULTANT_HOLIDAYS CH
                    JOIN 
                    Users U ON CH.CreatedBy = U.Id
                    WHERE 
                    (@SearchText IS NULL OR LOWER(CH.Name) LIKE '%' + LOWER(@SearchText) + '%')
                    AND (@Year IS NULL OR CH.Year = @Year)");

            parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
            parameters.Add("@Year", filtersAndPagination.Filters.Year, DbType.Int32);
            parameters.Add("@FieldToOrder", filtersAndPagination.OrderBy.FieldToOrder, DbType.String);
            parameters.Add("@DirectionOrder", filtersAndPagination.OrderBy.DirectionOrder, DbType.String);

            // Cuenta el número total de resultados sin aplicar la paginación
            var countQuery = "SELECT COUNT(*) FROM (" + queryBuilder.ToString() + ") AS TotalCountQuery;";
            var totalCount = await connection.ExecuteScalarAsync<int>(countQuery, parameters);

            // Aplica la paginación a la consulta
            queryBuilder.AppendLine(@"ORDER BY 
                    CASE WHEN @FieldToOrder = 'Year' AND @DirectionOrder = 'ASC' THEN CH.Year END ASC,
                    CASE WHEN @FieldToOrder = 'Year' AND @DirectionOrder = 'DESC' THEN CH.Year END DESC,
                    CASE WHEN @FieldToOrder = 'Name' AND @DirectionOrder = 'ASC' THEN CH.Name END DESC,
                    CASE WHEN @FieldToOrder = 'Name' AND @DirectionOrder = 'DESC' THEN CH.Name END DESC,
                    CH.Year");
            queryBuilder.AppendLine("OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;");

            parameters.Add("@Skip", (filtersAndPagination.Pagination.PageIndex - 1) * filtersAndPagination.Pagination.PageSize, DbType.Int32);
            parameters.Add("@Take", filtersAndPagination.Pagination.PageSize, DbType.Int32);

            var results = await connection.QueryAsync<HolidaysGetAllWithFiltersVM>(queryBuilder.ToString(), parameters);
            var holidays = results.ToList();

            return (holidays, totalCount);
        }


        public void Update(ConsultantHoliday obj)
        {
            _db.CONSULTANT_HOLIDAYS.Update(obj);
        }

    }
}
