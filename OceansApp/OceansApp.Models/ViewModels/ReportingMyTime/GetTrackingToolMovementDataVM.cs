
namespace OceansApp.Models.ViewModels.ReportingMyTime
{
    public class GetTrackingToolMovementDataVM
    {
        public DateTime ActionDate { get; set; }
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public string Notes { get; set; }
        public int MovementTypeId { get; set; }
        public string MovementTypeName { get; set; }
        public bool IsBillable { get; set; }
        public string? NonBillableReason { get; set; }
    }
}
