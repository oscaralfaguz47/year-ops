

namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class ListOfMovementsToDeferToNextPeriodVM
    {
        public int CostCenterId { get; set; }
        public int AccountingAccountId { get; set; }
        public string? Detail { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public int TransactionTypeId { get; set; }
    }
}
