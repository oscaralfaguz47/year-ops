namespace OceansApp.Models.ViewModels.TimeOff
{
    public class TimeOffWidgetVM
    {
        public List<TimeOffCalendarEntryVM> UpcomingApproved { get; set; } = new();
        public int PendingCount { get; set; }
    }
}
