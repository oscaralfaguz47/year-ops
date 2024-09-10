
using OceansApp.Models.ViewModels.Components.PaginationAndFilters;

namespace OceansApp.Models.ViewModels.JournalAccountsPayable
{
    public class JournalAccountsPayablePaginationFiltersVM
    {
        public JournalAccountsPayableFiltersGetAllVM? Filters { get; set; }
        public PaginationWithoutFiltersVM? PaginationWithoutFilters { get; set; }
    }
}
