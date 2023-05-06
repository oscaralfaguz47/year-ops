
namespace OceansApp.Models.ViewModels
{
    public class ProviderGroupByCategoryVM
    {
        public string IdCategory { get; set; }
        public string CategoryDescription { get; set; }
        public int NumProviders { get; set; }
        public List<ProviderGetAllVM> Providers { get; set; }
    }
}
