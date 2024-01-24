
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class Project
    {
        [Required]
        [Key]
        public int ProjectId { get; set; }
        [MaxLength(150)]
        public string Name { get; set; }
        [MaxLength(300)]
        public string? Description { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateLastUpdate { get; set; }
        [Required]
        public int ClientId { get; set; }
        [Required]
        public int SuccessManagerId { get; set; }

        [ValidateNever]
        public ConsultantDetail ConsultantDetail { get; set; }
        [ValidateNever]
        public Client Client { get; set; }
        [ValidateNever]
        public ApplicationUser? ApplicationUserUpdated { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUserCreated { get; set; }


    }
}
