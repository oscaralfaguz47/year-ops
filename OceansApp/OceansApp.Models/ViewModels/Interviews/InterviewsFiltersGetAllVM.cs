

namespace OceansApp.Models.ViewModels.Interviews
{
    public class InterviewsFiltersGetAllVM
    {
        public string? SearchText { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? TransactionStatusId { get; set; }
    }
}
