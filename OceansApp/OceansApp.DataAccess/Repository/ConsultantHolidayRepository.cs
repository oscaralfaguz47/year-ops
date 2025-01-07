using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using Dapper;
using System.Data;
using OceansApp.Models.ViewModels.Holidays;
using OceansApp.Models.ViewModels.Components;
using System.Linq.Expressions;
using OceansApp.Models.ViewModels.ConsultantHolidays;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantHolidayRepository : Repository<ConsultantHoliday>, IConsultantHolidayRepository
    {
        private ApplicationDbContext _db;
        public ConsultantHolidayRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<List<ConsultantHoliday>> GetAllAsync(Expression<Func<ConsultantHoliday, bool>>? predicate = null)
        {
            IQueryable<ConsultantHoliday> query = _db.CONSULTANT_HOLIDAYS;
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            return await query.ToListAsync();
        }

        public async Task<(List<HolidaysGetAllWithFiltersVM> holidays, int totalCount)> GetAllHolidaysWithFiltersAsync(HolidaysPaginationFiltersVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
            parameters.Add("@FieldToOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.FieldToOrder, DbType.String);
            parameters.Add("@DirectionOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.DirectionOrder, DbType.String);
            parameters.Add("@Skip", (filtersAndPagination.PaginationWithoutFilters.Pagination.PageIndex - 1) * filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@Take", filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var results = await connection.QueryAsync<HolidaysGetAllWithFiltersVM>("SP_CONSULTANT_HOLIDAYS_GetAllConsultantHolidaysWithFilters", parameters, commandType: CommandType.StoredProcedure);
            var totalCount = parameters.Get<int>("@TotalCount");

            var holidays = results.ToList();

            return (holidays, totalCount);
        }

        public async Task<CreateUpdateHolidayVM> GetConsultantHolidayWithDates(int consultantHolidayId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ConsultantHolidayId", consultantHolidayId);

            using (var multiResultSet = await connection.QueryMultipleAsync("SP_CONSULTANT_HOLIDAYS_GetConsultantHolidayWithDates", parameters, commandType: CommandType.StoredProcedure))
            {
                var consultantHoliday = await multiResultSet.ReadFirstOrDefaultAsync<ConsultantHoliday>();
                var holidayDates = await multiResultSet.ReadAsync<CreateUpdateHolidayDateVM>();

                return new CreateUpdateHolidayVM
                {
                    ConsultantHolidayId = consultantHoliday.ConsultantHolidayId,
                    Name = consultantHoliday.Name,
                    HolidayDates = (List<CreateUpdateHolidayDateVM>)holidayDates
                };
            }
        }

        public async Task<MethodResponse> CreateHolidayListWithHolidayDates(CreateUpdateHolidayVM holidayData)
        {
            try
            {
                using var transaction = await _db.Database.BeginTransactionAsync();

                ConsultantHoliday holidayListToCreate = new()
                {
                    Name = holidayData.Name.Trim(),
                    CreatedBy = holidayData.CreatedBy,
                    CreationDate = DateTime.UtcNow
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
                            Name = holiday.Name.Trim(),
                            Date = (DateTime)holiday.Date,
                            CreationDate = DateTime.UtcNow,
                            CreatedBy = holidayData.CreatedBy
                        };
                        await _db.CONSULTANT_HOLIDAY_DATES.AddAsync(holidayDateToCreate);
                    }
                    await _db.SaveChangesAsync();
                }
                else
                {
                    return new MethodResponse { MessageType = "Saving Error", Success = false, Message = $"The Holidays list could not be created. Something went wrong. Please report this issue." };
                }
                await transaction.CommitAsync();
                return new MethodResponse { Success = true, Message = $"The Holiday list was created successfully." };
            }
            catch (Exception ex)
            {
                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
            }
        }

        public async Task<MethodResponse> UpdateHolidayListWithHolidayDates(CreateUpdateHolidayVM holidayData, string updatedCreatedBy)
        {
            try
            {
                var holidayListToUpdate = await _db.CONSULTANT_HOLIDAYS.FirstOrDefaultAsync(x => x.ConsultantHolidayId == holidayData.ConsultantHolidayId);

                if (holidayListToUpdate == null)
                {
                    return new MethodResponse { MessageType = "No Exists Error", Success = false, Message = "The Holiday list no longer exists in the database." };
                }

                holidayListToUpdate.Name = holidayData.Name.Trim();

                List<ConsultantHolidayDate> existingHolidaysInListToRemove = await _db.CONSULTANT_HOLIDAY_DATES
                .Where(x => x.ConsultantHolidayId == holidayData.ConsultantHolidayId).ToListAsync();

                List<ConsultantHolidayDate> itemsToRemove = new List<ConsultantHolidayDate>();

                foreach (var holidayInListToAddOrUpdate in existingHolidaysInListToRemove)
                {
                    foreach (var holToUpdateCreate in holidayData.HolidayDates)
                    {
                        if (holidayInListToAddOrUpdate.ConsultantHolidayDateId == holToUpdateCreate.ConsultantHolidayDateId)
                        {
                            itemsToRemove.Add(holidayInListToAddOrUpdate);
                        }
                    }
                }

                foreach (var item in itemsToRemove)
                {
                    existingHolidaysInListToRemove.Remove(item);
                }
                using var transaction = await _db.Database.BeginTransactionAsync();

                if (existingHolidaysInListToRemove.Count > 0)
                {
                    _db.CONSULTANT_HOLIDAY_DATES.RemoveRange(existingHolidaysInListToRemove);
                }

                foreach (var holidayInListToAddOrUpdate in holidayData.HolidayDates)
                {
                    if (holidayInListToAddOrUpdate.ConsultantHolidayDateId == null)
                    {
                        ConsultantHolidayDate holidayDateToCreate = new()
                        {
                            ConsultantHolidayId = holidayListToUpdate.ConsultantHolidayId,
                            Name = holidayInListToAddOrUpdate.Name,
                            Date = (DateTime)holidayInListToAddOrUpdate.Date,
                            CreationDate = DateTime.UtcNow,
                            CreatedBy = updatedCreatedBy
                        };
                        await _db.CONSULTANT_HOLIDAY_DATES.AddAsync(holidayDateToCreate);
                    }
                    else
                    {
                        var existingHolidayInList = await _db.CONSULTANT_HOLIDAY_DATES
                        .FirstOrDefaultAsync(x => x.ConsultantHolidayDateId == holidayInListToAddOrUpdate.ConsultantHolidayDateId);
                        if (existingHolidayInList != null && (holidayInListToAddOrUpdate.Name != existingHolidayInList.Name ||
                            holidayInListToAddOrUpdate.Date != existingHolidayInList.Date))
                        {
                            existingHolidayInList.Name = holidayInListToAddOrUpdate.Name;
                            existingHolidayInList.Date = (DateTime)holidayInListToAddOrUpdate.Date;
                            existingHolidayInList.DateLastUpdate = DateTime.UtcNow;
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
                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
            }
        }

        public async Task<MethodResponse> DeleteHolidaysList(int holidaysListId)
        {
            try
            {
                var holidaysListToDelete = await _db.CONSULTANT_HOLIDAYS.FirstOrDefaultAsync(x => x.ConsultantHolidayId == holidaysListId);
                if (holidaysListToDelete == null)
                {
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = $"The Holidays list was not found in the database, it was removed before your request." };
                }

                int numConsultantsAssignedToHoliday = await _db.CONSULTANT_DETAILS.CountAsync(x=>x.ConsultantHolidayId == holidaysListToDelete.ConsultantHolidayId);

                if (numConsultantsAssignedToHoliday > 0)
                {
                    return MethodResponse.CreateFailureValidationResponse($"The holiday list you want to delete is associated to {numConsultantsAssignedToHoliday} consultant{(numConsultantsAssignedToHoliday > 1 ? "s":"")}, you cannot delete it.");
                }
                List<ConsultantHolidayDate> existingHolidaysInListToRemove = await _db.CONSULTANT_HOLIDAY_DATES
                .Where(x => x.ConsultantHolidayId == holidaysListId).ToListAsync();
                using var transaction = await _db.Database.BeginTransactionAsync();
                if (existingHolidaysInListToRemove.Count > 0)
                {
                    _db.CONSULTANT_HOLIDAY_DATES.RemoveRange(existingHolidaysInListToRemove);
                }
                _db.CONSULTANT_HOLIDAYS.Remove(holidaysListToDelete);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new MethodResponse { Success = true, Message = $"The Holidays list was deleted successfully." };
            }
            catch (Exception ex)
            {
                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
            }
        }

        public async Task<List<ConsultantHolidayDate>?> GetHolidaysDatesWhereConsultantInPeriodAsync(
     DateTime startDate, DateTime endDate, int? consultantHolidayId)
        {
            if (consultantHolidayId != null)
            {
                return await _db.CONSULTANT_HOLIDAY_DATES
              .Where(x => x.ConsultantHolidayId == consultantHolidayId
                          && x.Date >= startDate
                          && x.Date <= endDate)
              .ToListAsync();
            }
            else
            {
                return null;
            }
        }

        public async Task<List<GetHolidaysNameAndDateVM>> GetHolidaysByConsultantAsync(int consultantId, int year)
        {
            var result = await (from chd in _db.CONSULTANT_HOLIDAY_DATES
                                join ch in _db.CONSULTANT_HOLIDAYS on chd.ConsultantHolidayId equals ch.ConsultantHolidayId
                                join cd in _db.CONSULTANT_DETAILS on ch.ConsultantHolidayId equals cd.ConsultantHolidayId
                                where cd.ConsultantId == consultantId && chd.Date.Year == year
                                select new GetHolidaysNameAndDateVM
                                {
                                    HolidayName = chd.Name,
                                    Date = chd.Date
                                }).ToListAsync();
            return result;
        }



    }
}
