
namespace OceansApp.Models.ViewModels.ConsultantPaymentsDebitsCredits
{
    public class GetApprovedDebitsCreditsWhereConsultantVM
    {
        public int ConsultantPaymentDebitsCreditsId { get; set; }
        public string Detail { get; set; }
        public string TransactionTypeName { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }
    }
}
