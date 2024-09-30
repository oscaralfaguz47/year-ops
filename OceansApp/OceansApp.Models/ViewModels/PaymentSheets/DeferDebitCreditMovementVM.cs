
namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class DeferDebitCreditMovementVM
    {
        public int? Id { get; set; }
        public int? AccountingAccountId { get; set; }
        public int? CostCenterId { get; set; }
        public string? Description { get; set; }
    }
}
