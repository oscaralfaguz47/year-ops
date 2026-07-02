namespace OceansApp.Models.ViewModels.PaymentAnchor
{
    /// <summary>
    /// The payment anchor for a consultant who has no active project assignment in the period.
    /// Resolved from the consultant's most-recent historical assignment so that interviews
    /// (and other non-hours items) can be priced and accounted for against a real project.
    /// </summary>
    public class PaymentAnchorResult
    {
        public int ProjectId { get; set; }
        public int PositionId { get; set; }

        /// <summary>
        /// Hourly-equivalent rate taken from the most-recent history record with salary &gt; 0.
        /// </summary>
        public decimal Rate { get; set; }
    }
}
