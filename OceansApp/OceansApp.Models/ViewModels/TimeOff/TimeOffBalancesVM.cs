namespace OceansApp.Models.ViewModels.TimeOff
{
    public class TimeOffBalancesVM
    {
        // Consultant PTO (flat annual allowance)
        public bool IsPtoEnabled { get; set; }
        public decimal PtoAvailable { get; set; }
        public decimal PtoAnnualAllowance { get; set; }
        public int VtoAvailable { get; set; }

        // Admin PTO (monthly accrual)
        public bool IsAdminPtoEnabled { get; set; }
        public decimal AdminPtoAccruedToDate { get; set; }
        public decimal AdminPtoUsed { get; set; }
        public decimal AdminPtoAvailable { get; set; }
        public decimal AdminPtoMonthlyRate { get; set; }
    }
}
