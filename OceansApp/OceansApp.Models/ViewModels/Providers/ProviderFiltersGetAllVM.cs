
using OceansApp.Utility;

namespace OceansApp.Models.ViewModels.Providers
{
    public class ProviderFiltersGetAllVM
    {
        public string? IsActive { get; set; }
        public string? NameOrAlias { get; set; }
        public string? CountryId { get; set; }
        public int? ClientId { get; set; }
        public string? CompanyId { get; set; }
        public Pagination Pagination { get; set; }
        public List<ProviderGetAllWithFiltersVM>? ConsultantList { get; set; }
    }
}
