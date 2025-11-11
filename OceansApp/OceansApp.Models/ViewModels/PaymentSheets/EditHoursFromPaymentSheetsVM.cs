
namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class EditHoursFromPaymentSheetsVM
    {
        public int? MovementId { get; set; }
        public string? TimeFrom { get; set; }
        public string? TimeTo { get; set; }
        public decimal? Quantity { get; set; }
        public bool? Remove { get; set; } = false;
    }
}
