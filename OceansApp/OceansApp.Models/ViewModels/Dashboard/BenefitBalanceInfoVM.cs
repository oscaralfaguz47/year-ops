
namespace OceansApp.Models.ViewModels.Dashboard
{
    public class BenefitBalanceInfoVM
    {
        public decimal BalanceAmount { get; set; }
        public List<BenefitLastRequestsVM> LastRequests { get; set; }
    }
}
