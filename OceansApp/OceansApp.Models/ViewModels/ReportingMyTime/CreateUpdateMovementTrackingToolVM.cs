
namespace OceansApp.Models.ViewModels.ReportingMyTime
{
    public class CreateUpdateMovementTrackingToolVM 
    {
        public int? MovementId { get; set; }
        public int? ProjectId { get; set; }
        public DateTime? ActionDate { get; set; }
        public string? Notes { get; set; }
        public string? TimeFrom { get; set; }
        public string? TimeTo { get; set; }
        public int? MovementTypeId { get; set; }
        public bool? IsBillable { get; set; }
        public string? NonBillableReason { get; set; }
    }
}
