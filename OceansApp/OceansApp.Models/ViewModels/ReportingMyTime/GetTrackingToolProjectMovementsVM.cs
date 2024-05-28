
namespace OceansApp.Models.ViewModels.ReportingMyTime
{
    public class GetTrackingToolProjectMovementsVM
    {
        public int MovementId { get; set; }
        public DateTime ActionDate { get; set; }
        public string? Notes { get; set; }
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public string TransactionStatusName { get; set; }
    }
}
