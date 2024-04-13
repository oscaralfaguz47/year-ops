
namespace OceansApp.Models.ViewModels.ReportingMyTime
{
    public class CreateUpdateMovementClientNoTrackingToolVM
    {
        public int? MovementId { get; set; }
        public int? ProjectId { get; set; }
        public DateTime? ActionDate { get; set; }
        public decimal? Quantity { get; set; }
        public string? Notes { get; set; }
        public string? MovementType { get; set; }
    }
}
