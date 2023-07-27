
namespace OceansApp.Models.ViewModels.DocumentsCC
{
    public class DocumentCCFiltersGetAllVM
    {
        public string? SearchText { get; set; }
        public int? ClientId { get; set; }
        public string? CompanyId { get; set; }
        public string? DocumentType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
