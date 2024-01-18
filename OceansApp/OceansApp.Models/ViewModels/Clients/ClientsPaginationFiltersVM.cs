
using OceansApp.Models.ViewModels.Components.PaginationAndFilters;

namespace OceansApp.Models.ViewModels.Clients
{
    public class ClientsPaginationFiltersVM
    {
        public ClientsFiltersGetAllVM? Filters { get; set; }
        public PaginationWithoutFiltersVM? PaginationWithoutFilters { get; set; }
    }
}
