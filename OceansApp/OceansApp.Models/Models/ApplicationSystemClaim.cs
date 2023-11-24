using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class ApplicationSystemClaim
    {
        [Key]
        public int ClaimId { get; set; }
        [Required]
        public string ClaimType { get; set; }
        [Required]
        public string ClaimValue { get; set; }
        [MaxLength(450)]
        public string Description { get; set; }
        [Required]
        public int SystemSubAreaId { get; set; }
        [ForeignKey("SystemSubAreaId")]
        [ValidateNever]
        public SystemSubArea SystemSubArea { get; set; }
    }
}
