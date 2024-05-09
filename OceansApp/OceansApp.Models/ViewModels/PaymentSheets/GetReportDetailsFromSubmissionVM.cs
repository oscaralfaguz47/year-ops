namespace OceansApp.Models.ViewModels.PaymentSheets
{
    public class GetReportDetailsFromSubmissionVM
    {
        public DateTime SubmissionDate { get; set; }
        public DateTime? LastSubmissionDate { get; set; }
        public string ProjectName { get; set; }
        public bool ClientHasTrackingTool { get; set; }
        public string ConsultantName { get; set; }
        public DateTime StartPeriodDate { get; set; }
        public DateTime EndPeriodDate { get; set; }
        public string Movements { get; set; }
    }
}
