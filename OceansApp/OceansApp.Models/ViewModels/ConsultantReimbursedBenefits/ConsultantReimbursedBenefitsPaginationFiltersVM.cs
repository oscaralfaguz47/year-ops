
using OceansApp.Models.ViewModels.Components.PaginationAndFilters;

namespace OceansApp.Models.ViewModels.ConsultantReimbursedBenefits
{
    public class ConsultantReimbursedBenefitsPaginationFiltersVM
    {
        public ConsultantReimbursedBenefitsFiltersGetAllVM? Filters { get; set; }
        public PaginationWithoutFiltersVM? PaginationWithoutFilters { get; set; }
    }
}
