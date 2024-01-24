
namespace OceansApp.Models.ViewModels.Projects
{
     public class ProjectsFiltersGetAllVM
    {
        public string? SearchText { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? IsActive { get; set; }
        public int? ClientId { get; set; }
        public int? SuccessManagerId { get; set; }
    }
}
