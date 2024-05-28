
namespace OceansApp.Models.ViewModels.ConsultantPaymentsDebitsCredits
{
    public class CreateUpdateConsultantPaymentDebitCreditVM
    {
        public int? ConsultantPaymentDebitsCreditsId { get; set; }
        public int? ConsultantId { get; set; }
        public string? ConsultantName { get; set; }
        public string? ConsultantEmail { get; set; }
        public string? ConsultantCompanyId { get; set; }
        public int? AccountingAccountId { get; set; }
        public int? CostCenterId { get; set; }
        public string? CostCenterName { get; set; }
        public string? Detail { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? ActionDateWithinFortnight { get; set; }
        public string? TransactionTypeName { get; set; }
    }
}
