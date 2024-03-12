
namespace OceansApp.Models.ViewModels.Projects
{
    public class CreateUpdateProjectVM
    {
        public int? ProjectId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? StartDate { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsBillable { get; set; }
        public int? ClientId { get; set; }
        public string? ClientName { get; set; }
        public int? SuccessManagerId { get; set; }
        public string? SuccessManagerName { get; set; }
        public bool? ClientHasTrackingTool { get; set; }
        public List<CreateUpdateProjectConsultantAssignedVM>? AssignedConsultants { get; set; }
        public string? CreatedBy { get; set; }
        public string? ProjectType { get; set; }
    }
}
