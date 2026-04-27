namespace OceansApp.Models.ViewModels.TimeOff
{
    public class TimeOffCalendarEntryVM
    {
        public int TimeOffRequestId { get; set; }
        public string TimeOffType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }
}
