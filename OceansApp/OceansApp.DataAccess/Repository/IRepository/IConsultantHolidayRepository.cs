
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Holidays;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantHolidayRepository : IRepository<ConsultantHoliday> 
    {
        void Update(ConsultantHoliday obj);
        Task<(List<HolidaysGetAllWithFiltersVM> holidays, int totalCount)> GetAllHolidaysWithFiltersAsync(HolidaysPaginationFiltersVM filtersAndPagination);
    }
}
