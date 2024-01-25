
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ProjectConsultantAssignedHistory
    {
        [Required]
        public int ProjectConsultantAssignedId { get; set; }
        [MaxLength(50)]
        [Required]
        public string Action { get; set; }
        [Required]
        public DateTime ActionDate { get; set; }
        [Required]
        [MaxLength(450)]
        public string UserActionedBy { get; set; }

        [ValidateNever]
        public ProjectConsultantAssigned ProjectConsultantAssigned { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUserActionedBy { get; set; }
        
    }
}
