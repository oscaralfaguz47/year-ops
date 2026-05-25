namespace OceansApp.Models.ViewModels.TimeOff
{
    public class TimeOffBalancesVM
    {
        public bool IsPtoEnabled { get; set; }
        public decimal PtoAvailable { get; set; }
        public decimal PtoAnnualAllowance { get; set; }
        public int VtoAvailable { get; set; }
    }
}
