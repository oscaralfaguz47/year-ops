namespace OceansApp.Models.ViewModels.ProjectConsultantAssigned
{
    public class GetConsultantSelectedProjectInfoVM
    {
        public int PaymentPeriod { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public bool ClientHasTrackingTool { get; set; }
        public string SuccessManagerName { get; set; }
        public string SuccessManagerEmail { get; set; }
        public bool ParticipatesInOnCalls { get; set; }
        public int NumAssignedProjects { get; set; }
    }
}
