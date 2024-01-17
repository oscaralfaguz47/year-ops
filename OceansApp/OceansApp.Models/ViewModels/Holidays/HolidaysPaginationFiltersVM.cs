
using OceansApp.Models.ViewModels.Components.PaginationAndFilters;

namespace OceansApp.Models.ViewModels.Holidays
{
    public class HolidaysPaginationFiltersVM
    {
        public HolidaysFiltersGetAllVM? Filters { get; set; }
        public PaginationWithoutFiltersVM? PaginationWithoutFilters { get; set; }
    }
}
