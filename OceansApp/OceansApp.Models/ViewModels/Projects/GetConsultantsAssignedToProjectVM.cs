
namespace OceansApp.Models.ViewModels.Projects
{
    public class GetConsultantsAssignedToProjectVM
    {
        public int ProjectConsultantAssignedId { get; set; }
        public string ConsultantName { get; set; }
        public bool IsActive { get; set; }
        public DateTime BeforeOrAfterStatusActionDate { get; set; }
        public string UserCategory { get; set; }
    }
}
