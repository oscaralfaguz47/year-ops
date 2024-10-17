
namespace OceansApp.Models.ViewModels.ReportingMyTimeSubmissions
{
    public class LastTimesheetSubmittedVM
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ProjectName { get; set; }
        public string Status { get; set; }
        public decimal TotalHours { get; set; }
    }
}
