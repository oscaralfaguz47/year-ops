using OceansApp.Models.ViewModels.Bonusly;

namespace OceansApp.Models.ViewModels.Dashboard
{
    public class BonuslyBalanceInfoVM
    {
        public decimal BalanceAmount { get; set; }
        public List<RedemptionsVM> LastRequests { get; set; }
    }
}
