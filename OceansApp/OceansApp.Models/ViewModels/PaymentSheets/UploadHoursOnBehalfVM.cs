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
        public decimal Hours { get; set; }
    }
}
