
namespace OceansApp.Models.ViewModels.ConsultantPayments
{
    public class CreateUpdateConsultantPaymentVM
    {
        public int? ConsultantPaymentId { get; set; }
        public int? ConsultantId { get; set; }
        public string? StartDatePeriod { get; set; }
        public string? EndDatePeriod { get; set; }
        public string? ReferenceNumber { get; set; }
        public int? PaymentMethodId { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string? AccountingDate { get; set; }
        public string? CompanyId { get; set; }
        public int? BankAccountId { get; set; }
    }
}
