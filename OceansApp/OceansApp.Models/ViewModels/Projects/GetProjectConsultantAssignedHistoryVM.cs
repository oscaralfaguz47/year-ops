
namespace OceansApp.Models.ViewModels.Projects
{
    public class GetProjectConsultantAssignedHistoryVM
    {
        public DateTime ActionDate { get; set; }
        public string UserActionedBy { get; set; }
        public decimal? NewValue { get; set; }
        public decimal? OldValue { get; set; }
        public string Action { get; set; }
        public string? NewValueDetail { get; set; }
        public string? OldValueDetail { get; set; }
        public string UserCategory { get; set; }
    }
}
