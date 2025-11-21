
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ConsultantBenefitReset
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(100)]
        [Required]
        public string Description { get; set; }
        [Required]
        public string ArrayResetValues { get; set; }
        [Required]
        public DateTime ResetDate { get; set; }
        [Required]
        [MaxLength(450)]
        public string UserIdCreatedBy { get; set; }
        [Required]
        public int YearToReset { get; set; }

        [ValidateNever]
        public ApplicationUser ApplicationUserCreatedBy { get; set; }
    }
}
