
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ProjectConsultantAssigned
    {
        [Key]
        [Required]
        public int ProjectConsultantAssignedId { get; set; }
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public int ConsultantId { get; set; }
        [Required]


        [ValidateNever]
        public ConsultantDetail ConsultantDetail { get; set; }
        [ValidateNever]
        public Project Project { get; set; }
    }
}
