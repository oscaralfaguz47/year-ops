
namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class PaymentSheetsGetAllWithFiltersVM
    {
        public string ConsultantName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int SubmissionId { get; set; }
        public string TransactionStatusName { get; set; }
        public DateTime SubmissionDate { get; set; }
        public DateTime LastSubmissionDate { get; set; }
    }
}
