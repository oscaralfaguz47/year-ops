
using OceansApp.Models.ViewModels.Components;

namespace OceansApp.Models.ViewModels.Holidays
{
    public class HolidaysPaginationFiltersVM
    {
        public HolidaysFiltersGetAllVM? Filters { get; set; }
        public Pagination? Pagination { get; set; }
    }
}
