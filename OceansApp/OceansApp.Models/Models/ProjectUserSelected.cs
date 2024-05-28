using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ProjectUserSelected
    {
        [Required]
        public int ProjectId { get; set; }
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }

        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        [ValidateNever]
        public Project Project { get; set; }
    }
}
