using OceansApp.Utility;

namespace OceansApp.Models.ViewModels.Providers
{
    public class ProviderGetAllForListVM
    {
        public ProviderFiltersGetAllVM Filters { get; set; }
        public Pagination Pagination { get; set; }
        public List<ProviderGetAllWithFiltersVM>? ConsultantList { get; set; }
    }
}
