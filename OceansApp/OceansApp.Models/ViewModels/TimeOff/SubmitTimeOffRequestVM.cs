namespace OceansApp.Models.ViewModels.TimeOff
{
    public class SubmitTimeOffRequestVM
    {
        public string? TimeOffType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
