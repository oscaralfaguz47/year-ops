
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ProjectConsultantAssignedHistory
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public int ProjectConsultantAssignedId { get; set; }
        [MaxLength(30)]
        [Required]
        public string Action { get; set; }
        [MaxLength(130)]
        public string? OldValue { get; set; }
        [MaxLength(130)]
        public string? NewValue { get; set; }
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
