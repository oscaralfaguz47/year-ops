
using OceansApp.Models.ViewModels.Components.PaginationAndFilters;

namespace OceansApp.Models.ViewModels.ConsultantPositions
{
    public class ConsultantPositionsPaginationFiltersVM
    {
        public ConsultantPositionsFiltersGetAllVM? Filters { get; set; }
        public PaginationWithoutFiltersVM? PaginationWithoutFilters { get; set; }
    }
}
