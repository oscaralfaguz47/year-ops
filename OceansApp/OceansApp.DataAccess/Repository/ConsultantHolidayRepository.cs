using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using Dapper;
using System.Data;
using OceansApp.Models.ViewModels.Holidays;
using OceansApp.Models.ViewModels.Components;

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

        public async Task<CreateUpdateHolidayVM> GetConsultantHolidayWithDates(int consultantHolidayId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ConsultantHolidayId", consultantHolidayId);

            using (var multiResultSet = await connection.QueryMultipleAsync("GetConsultantHolidayWithDates", parameters, commandType: CommandType.StoredProcedure))
            {
                var consultantHoliday = await multiResultSet.ReadFirstOrDefaultAsync<ConsultantHoliday>();
                var holidayDates = await multiResultSet.ReadAsync<ConsultantHolidayDate>();

                return new CreateUpdateHolidayVM
                {
                    ConsultantHolidayId = consultantHoliday.ConsultantHolidayId,
                    Name = consultantHoliday.Name,
                    Year = consultantHoliday.Year,
                    HolidayDates = (List<CreateUpdateHolidayDateVM>)holidayDates
                };
            }
        }


        public async Task<MethodResponse> CreateHolidayListWithHolidayDates(CreateUpdateHolidayVM holidayData)
        {
            try
            {
                var existingHoliday = await _db.CONSULTANT_HOLIDAYS.FirstOrDefaultAsync(x => x.Name == holidayData.Name.Trim());
                var costaRicaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central America Standard Time");
                if (existingHoliday != null)
                {
                    return new MethodResponse { Success = false, Message = $"There is already a list of Holidays with the name '{holidayData.Name.Trim()}'." };
                }
                using var transaction = await _db.Database.BeginTransactionAsync();

                ConsultantHoliday holidayListToCreate = new()
                {
                    Name = holidayData.Name,
                    Year = holidayData.Year,
                    CreatedBy = holidayData.CreatedBy,
                    CreationDate = costaRicaTime
                };
                var createdHolidayList = await _db.CONSULTANT_HOLIDAYS.AddAsync(holidayListToCreate);
                await _db.SaveChangesAsync();

                if (createdHolidayList.Entity != null && createdHolidayList.Entity.ConsultantHolidayId > 0)
                {
                    foreach (var holiday in holidayData.HolidayDates)
                    {
                        ConsultantHolidayDate holidayDateToCreate = new()
                        {
                            ConsultantHolidayId = createdHolidayList.Entity.ConsultantHolidayId,
                            Name = holiday.Name,
                            Date = holiday.Date,
                            CreationDate = costaRicaTime,
                            CreatedBy = holidayData.CreatedBy
                        };
                        await _db.CONSULTANT_HOLIDAY_DATES.AddAsync(holidayDateToCreate);
                    }
                    await _db.SaveChangesAsync();
                }
                else
                {
                    return new MethodResponse { Success = false, Message = $"The Holidays list could not be created." };
                }
                await transaction.CommitAsync();
                return new MethodResponse { Success = true, Message = $"The Holiday list was created successfully." };
            }
            catch (Exception ex)
            {
                return new MethodResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<MethodResponse> UpdateHolidayListWithHolidayDates(CreateUpdateHolidayVM holidayData, string updatedCreatedBy)
        {
            try
            {
                var existingHoliday = await _db.CONSULTANT_HOLIDAYS.FirstOrDefaultAsync(x => x.Name == holidayData.Name.Trim());
                var costaRicaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central America Standard Time");
                if (existingHoliday != null)
                {
                    return new MethodResponse { Success = false, Message = $"There is already a list of Holidays with the name '{holidayData.Name.Trim()}'." };
                }

                var holidayListToUpdate = await _db.CONSULTANT_HOLIDAYS.FirstOrDefaultAsync(x => x.ConsultantHolidayId == holidayData.ConsultantHolidayId);

                if (holidayListToUpdate == null)
                {
                    return new MethodResponse { Success = false, Message = "The Holiday list no longer exists in the database." };
                }

                holidayListToUpdate.Name = holidayData.Name.Trim();
                holidayListToUpdate.Year = holidayData.Year;

                List<ConsultantHolidayDate> existingHolidaysInListToRemove = await _db.CONSULTANT_HOLIDAY_DATES
                    .Where(x => x.ConsultantHolidayId == holidayData.ConsultantHolidayId).ToListAsync();

                foreach (var holidayInListToAddOrUpdate in existingHolidaysInListToRemove)
                {
                    foreach (var holToUpdateCreate in holidayData.HolidayDates)
                    {
                        if (holidayInListToAddOrUpdate.ConsultantHolidayDateId == holToUpdateCreate.ConsultantHolidayDateId)
                        {
                            existingHolidaysInListToRemove.Remove(holidayInListToAddOrUpdate);
                        }
                    }
                }
                using var transaction = await _db.Database.BeginTransactionAsync();

                _db.CONSULTANT_HOLIDAY_DATES.RemoveRange(existingHolidaysInListToRemove);

                foreach (var holidayInListToAddOrUpdate in holidayData.HolidayDates)
                {
                    if (holidayInListToAddOrUpdate.ConsultantHolidayDateId == null)
                    {
                        ConsultantHolidayDate holidayDateToCreate = new()
                        {
                            ConsultantHolidayId = holidayListToUpdate.ConsultantHolidayId,
                            Name = holidayInListToAddOrUpdate.Name,
                            Date = holidayInListToAddOrUpdate.Date,
                            CreationDate = costaRicaTime,
                            CreatedBy = updatedCreatedBy
                        };
                    }
                    else
                    {
                        var existingHolidayInList = await _db.CONSULTANT_HOLIDAY_DATES
                        .FirstOrDefaultAsync(x => x.ConsultantHolidayDateId == holidayInListToAddOrUpdate.ConsultantHolidayDateId);
                        if (existingHolidayInList != null)
                        {
                            existingHolidayInList.Name = holidayInListToAddOrUpdate.Name;
                            existingHolidayInList.Date = holidayInListToAddOrUpdate.Date;
                            existingHolidayInList.DateLastUpdate = costaRicaTime;
                            existingHolidayInList.UpdatedBy = updatedCreatedBy;
                        }
                    }
                }
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new MethodResponse { Success = true, Message = $"The Holiday list was updated successfully." };
            }
            catch (Exception ex)
            {
                return new MethodResponse { Success = false, Message = ex.Message };
            }
        }

        public void Update(ConsultantHoliday obj)
        {
            _db.CONSULTANT_HOLIDAYS.Update(obj);
        }

    }
}
