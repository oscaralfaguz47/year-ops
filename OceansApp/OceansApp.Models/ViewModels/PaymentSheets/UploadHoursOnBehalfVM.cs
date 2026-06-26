namespace OceansApp.Models.ViewModels.PaymentSheets
{
    /// <summary>
    /// Request payload for the admin Manual Hours Upload action — hours filed on behalf of a
    /// consultant for one pay period. See docs/adr/0002.
    /// </summary>
    public class UploadHoursOnBehalfVM
    {
        public int ConsultantId { get; set; }
        public int ProjectId { get; set; }
        public DateTime StartPeriodDate { get; set; }
        public DateTime EndPeriodDate { get; set; }
        /// <summary>Hours worked per weekday (autofill semantics) — written to each weekday in the period, not a period total.</summary>
        public decimal HoursPerDay { get; set; }
    }
}
