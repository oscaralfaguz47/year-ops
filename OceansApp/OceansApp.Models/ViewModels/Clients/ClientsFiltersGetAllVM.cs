
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.ViewModels.Clients
{
    public class ClientsFiltersGetAllVM
    {
        public string? SearchText { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? IsActive { get; set; }
        public string? CompanyId { get; set; }
        public int? SuccessManagerId { get; set; }
    }
}
