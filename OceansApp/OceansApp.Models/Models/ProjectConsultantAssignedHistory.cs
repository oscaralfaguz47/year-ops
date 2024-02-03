
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
        [Required]
        public int ActionId { get; set; }
        public decimal? OldValue { get; set; }
        public decimal? NewValue { get; set; }
        [MaxLength(130)]
        public string? OldValueDetail { get; set; }
        [MaxLength(130)]
        public string? NewValueDetail { get; set; }
        [Required]
        public DateTime ActionDate { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        public int UserActionedBy { get; set; }

        [ValidateNever]
        public ProjectConsultantAssigned ProjectConsultantAssigned { get; set; }
        [ValidateNever]
        public ConsultantDetail ConsultantUserActionedBy { get; set; }
        [ValidateNever]
        public ProjectConsultantAssignedHistoryAction Action { get; set; }

    }
}
