
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ProjectConsultantPendingSubmission
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public int ConsultantId { get; set; }
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }

        [ValidateNever]
        public ConsultantDetail ConsultantDetail { get; set; }
        [ValidateNever]
        public Project Project { get; set; }
    }
}
