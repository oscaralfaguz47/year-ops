
namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class PaymentSheetsFiltersGetAllVM
    {
        public string? SearchText { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? TransactionStatusName { get; set; }
        public int? ProjectId { get; set; }
        public int? PaymentPeriod { get; set; }
    }
}
