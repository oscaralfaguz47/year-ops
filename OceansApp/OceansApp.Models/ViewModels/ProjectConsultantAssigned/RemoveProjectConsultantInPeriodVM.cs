
namespace OceansApp.Models.ViewModels.ProjectConsultantAssigned
{
    public class RemoveProjectConsultantInPeriodVM
    {
        // Nullable so a stray project-less (null ProjectId) payload binds and fails as a clean
        // "ProjectId is required" validation error instead of an unhandled JSON deserialization 400.
        public int? ProjectId { get; set; }
        public int ConsultantId { get; set; }
        public DateTime StartPeriodDate { get; set; }
        public DateTime EndPeriodDate { get; set; }
    }
}
