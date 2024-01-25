
namespace OceansApp.Models.ViewModels.Projects
{
    public class ProjectsGetAllWithFiltersVM
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsActive { get; set; }
        public string ClientName { get; set; }
        public string SuccessManagerName { get; set; }
        public int NumConsultantsAssigned { get; set; }
    }
}
