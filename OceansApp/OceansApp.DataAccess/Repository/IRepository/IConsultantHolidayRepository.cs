
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Holidays;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantHolidayRepository : IRepository<ConsultantHoliday> 
    {
        Task<List<int>> GetHolidaysYears();
        void Update(ConsultantHoliday obj);
        Task<(List<HolidaysGetAllWithFiltersVM> holidays, int totalCount)> GetAllHolidaysWithFiltersAsync(HolidaysPaginationFiltersVM filtersAndPagination);
    }
}
