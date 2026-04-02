namespace OceansApp.Models.ViewModels.TimeOff
{
    public class TimeOffRequestListVM
    {
        public int TimeOffRequestId { get; set; }
        public string TimeOffType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int BusinessDays { get; set; }
        public string Status { get; set; }
        public string ConsultantName { get; set; }
        public int ConsultantId { get; set; }
        public string? ManagerName { get; set; }
        public DateTime? ActionDate { get; set; }
        public string? RejectionComment { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
