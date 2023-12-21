
using OceansApp.Models.ViewModels.Components;

namespace OceansApp.Models.ViewModels.Holidays
{
    public class HolidaysGetAllForListVM
    {
        public HolidaysPaginationFiltersVM PaginationFilters { get; set; }
        public List<HolidaysGetAllWithFiltersVM>? HolidaysList { get; set; }
    }
}
