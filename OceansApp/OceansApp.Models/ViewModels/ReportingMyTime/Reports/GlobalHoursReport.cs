
namespace OceansApp.Models.ViewModels.ReportingMyTime.Reports
{
    public class GlobalHoursReport
    {
        public string ConsultantName { get; set; }
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public DateTime ActionDate { get; set; }
        public string? TimeFrom { get; set; }
        public string? TimeTo { get; set; }
        public decimal Quantity { get; set; }
        public string? Notes { get; set; }
        public string? MovementType { get; set; }
    }
}
