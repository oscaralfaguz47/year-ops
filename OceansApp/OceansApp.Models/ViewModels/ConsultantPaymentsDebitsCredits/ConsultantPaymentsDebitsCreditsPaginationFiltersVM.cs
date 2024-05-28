
using OceansApp.Models.ViewModels.Components.PaginationAndFilters;

namespace OceansApp.Models.ViewModels.ConsultantPaymentsDebitsCredits
{
    public class ConsultantPaymentsDebitsCreditsPaginationFiltersVM
    {
        public ConsultantPaymentsDebitsCreditsFiltersGetAllVM? Filters { get; set; }
        public PaginationWithoutFiltersVM? PaginationWithoutFilters { get; set; }
    }
}
