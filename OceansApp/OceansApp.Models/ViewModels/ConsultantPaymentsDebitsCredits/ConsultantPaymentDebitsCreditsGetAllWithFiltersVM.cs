
namespace OceansApp.Models.ViewModels.ConsultantPaymentsDebitsCredits
{
    public class ConsultantPaymentDebitsCreditsGetAllWithFiltersVM
    {
        public int ConsultantPaymentDebitsCreditsId { get; set; }
        public string ConsultantName { get; set; }
        public string AccountingAccountName { get; set; }
        public string CostCenterName { get; set; }
        public string? Detail { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public DateTime ActionDateWithinFortnight { get; set; }
        public string TransactionStatusName { get; set; }
        public string TransactionTypeName { get; set; }
        public DateTime CreationDate { get; set; }
        public string UserCreatedBy { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? LastUpdatedBy { get; set; }
    }
}
