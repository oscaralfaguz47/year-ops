namespace OceansApp.Models.ViewModels.ProjectConsultantAssigned
{
    public class GetConsultantSelectedProjectInfoVM
    {
        public int PaymentPeriod { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public bool ClientHasTrackingTool { get; set; }
        public string SucessManagerName { get; set; }
        public string SuccessManagerEmail { get; set; }
        public bool ParticipatesInOnCalls { get; set; }
        public int? PartnerId { get; set; }
        public bool AccessToTrackingTool { get; set; }
        public int NumAssignedProjects { get; set; }
    }
}
