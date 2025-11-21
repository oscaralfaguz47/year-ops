
using OceansApp.Models.ViewModels.Components.PaginationAndFilters;

namespace OceansApp.Models.ViewModels.ConsultantsAndBenefits
{
    public class ConsultantsAndBenefitsBalancePaginationFiltersVM
    {
        public ConsultantsAndBenefitsBalanceFiltersGetAllVM? Filters { get; set; }
        public PaginationWithoutFiltersVM? PaginationWithoutFilters { get; set; }
    }
}
