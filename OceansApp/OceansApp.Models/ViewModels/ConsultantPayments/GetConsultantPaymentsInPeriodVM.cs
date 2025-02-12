
namespace OceansApp.Models.ViewModels.ConsultantPayments
{
    public class GetConsultantPaymentsInPeriodVM
    {
        public int ConsultantPaymentId { get; set; }
        public string ReferenceNumber { get; set; }
        public string PaymentMethodName { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime AccountingDate { get; set; }
        public string CompanyId { get; set; }
        public string BankAccountName { get; set; }
        public string? UserCreatedBy { get; set; }
        public string? UserUpdatedBy { get; set; }
    }
}
