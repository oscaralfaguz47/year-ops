
namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class GetPaymentDetailsMovementsVM
    {
        public int? MovementId { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int? MovementTypeId { get; set; }
        public string MovementTypeName { get; set; }
        public string? PaymentType { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount => decimal.Round(Quantity * UnitPrice, 2);
    }
}
