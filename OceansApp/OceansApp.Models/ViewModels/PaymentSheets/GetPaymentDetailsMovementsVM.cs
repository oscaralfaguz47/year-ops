
namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class GetPaymentDetailsMovementsVM
    {
        public string ProjectName { get; set; }
        public string MovementTypeName { get; set; }
        public string PaymentType { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount => Quantity * UnitPrice;
    }
}
