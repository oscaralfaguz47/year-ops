using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ProjectConsultantPeriodDisabledTracking
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public int ConsultantId { get; set; }
        [Required]
        public DateTime StartPeriodDate { get; set; }
        [Required]
        public DateTime EndPeriodDate { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        [MaxLength(450)]
        public string CreatedBy { get; set; }


        [ValidateNever]
        public Project Project { get; set; }
        [ValidateNever]
        public ConsultantDetail ConsultantDetail { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUserCreated { get; set; }
    }
}
