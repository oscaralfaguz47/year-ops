namespace OceansApp.Models.ViewModels.PaymentAnchor
{
    /// <summary>
    /// A single assignment-history row (joined to its assignment's project) consumed by the
    /// Payment Anchor Resolver. Kept deliberately small so the resolution logic can be unit
    /// tested without a database.
    /// </summary>
    public class PaymentAnchorHistoryRecord
    {
        public int ProjectConsultantAssignedId { get; set; }
        public int ProjectId { get; set; }
        public int PositionId { get; set; }
        public decimal? HourlySalary { get; set; }
        public decimal? MonthlySalary { get; set; }
        public DateTime ActionDate { get; set; }

        /// <summary>History primary key, used only as a deterministic tie-breaker on equal ActionDate.</summary>
        public int Id { get; set; }
    }
}
