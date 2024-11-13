
namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class DeferDebitCreditVM
    {
        public DateTime? ActionDate { get; set; }
        public int? ConsultantId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<DeferDebitCreditMovementVM?> MovementsList { get; set; }
    }
}
