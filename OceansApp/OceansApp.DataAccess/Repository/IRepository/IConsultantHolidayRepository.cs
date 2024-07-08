
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Holidays;
using System.Linq.Expressions;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantHolidayRepository : IRepository<ConsultantHoliday> 
    {
        Task<List<ConsultantHoliday>> GetAllAsync(Expression<Func<ConsultantHoliday, bool>>? predicate = null);
        void Update(ConsultantHoliday obj);
        Task<(List<HolidaysGetAllWithFiltersVM> holidays, int totalCount)> GetAllHolidaysWithFiltersAsync(HolidaysPaginationFiltersVM filtersAndPagination);
        Task<CreateUpdateHolidayVM> GetConsultantHolidayWithDates(int consultantHolidayId);
        Task<MethodResponse> CreateHolidayListWithHolidayDates(CreateUpdateHolidayVM holidayData);
        Task<MethodResponse> UpdateHolidayListWithHolidayDates(CreateUpdateHolidayVM holidayData, string updatedCreatedBy);
        Task<MethodResponse> DeleteHolidaysList(int holidaysListId);
    }
}
