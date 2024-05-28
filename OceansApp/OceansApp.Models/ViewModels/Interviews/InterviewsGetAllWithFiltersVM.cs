
namespace OceansApp.Models.ViewModels.Interviews
{
    public class InterviewsGetAllWithFiltersVM
    {
        public int InterviewId { get; set; }
        public string ConsultantName { get; set; }
        public decimal DurationMinutes { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? LastUpdatedBy { get; set; }
        public string TransactionStatusName { get; set; }
    }
}
