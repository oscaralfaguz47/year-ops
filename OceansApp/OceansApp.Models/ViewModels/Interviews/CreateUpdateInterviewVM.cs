
namespace OceansApp.Models.ViewModels.Interviews
{
    public class CreateUpdateInterviewVM
    {
        public int? InterviewId { get; set; }
        public int? ConsultantId { get; set; }
        public string? ConsultantName { get; set; }
        public string? ConsultantEmail { get; set; }
        public decimal? DurationMinutes { get; set; }
        public DateTime? Date { get; set; }
        public string? Detail { get; set; }

    }
}
