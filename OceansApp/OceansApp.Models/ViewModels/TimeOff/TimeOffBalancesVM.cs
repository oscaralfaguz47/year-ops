namespace OceansApp.Models.ViewModels.TimeOff
{
    public class TimeOffBalancesVM
    {
        public bool IsPtoEnabled { get; set; }
        public int PtoAvailable { get; set; }
        public int PtoAnnualAllowance { get; set; }
        public int VtoAvailable { get; set; }
    }
}
