
namespace OceansApp.Models.ViewModels.ProjectConsultantAssigned
{
    public class RemoveProjectConsultantInPeriodVM
    {
        public int ProjectId { get; set; }
        public int ConsultantId { get; set; }
        public DateTime StartPeriodDate { get; set; }
        public DateTime EndPeriodDate { get; set; }
    }
}
