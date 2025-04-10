
using OceansApp.Models.ViewModels.Components.PaginationAndFilters;

namespace OceansApp.Models.ViewModels.DocumentsCC
{
    public class DocumentCCPaginationFiltersVM
    {
            public DocumentCCFiltersGetAllVM? Filters { get; set; }
            public PaginationWithoutFiltersVM? PaginationWithoutFilters { get; set; }
    }
}
