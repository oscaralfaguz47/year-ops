namespace OceansApp.Models.ViewModels.PaymentSheets
{
    /// <summary>
    /// Request payload for the admin Manual Hours Upload action — hours filed on behalf of a
    /// consultant for one pay period. See docs/adr/0002 and docs/adr/0003.
    /// </summary>
    public class UploadHoursOnBehalfVM
    {
        public int ConsultantId { get; set; }
        public int ProjectId { get; set; }
        public DateTime StartPeriodDate { get; set; }
        public DateTime EndPeriodDate { get; set; }
        /// <summary>
        /// Total hours to file for the whole period. The server recomputes the workable-day count and
        /// spreads this total across those days (even to the cent, trailing days carry the remainder);
        /// the client never sends a per-day rate or day count. See docs/adr/0003.
        /// </summary>
        public decimal TotalHours { get; set; }
    }
}
