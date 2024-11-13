
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class GlobalConsecutive
    {
        [Key]
        [Required]
        public int GlobalConsecutiveId { get; set; }
        [MaxLength(80)]
        [Required]
        public string Name { get; set; }
        [Required]
        public int ConsecutiveNumber { get; set; }
        [Required]
        [MaxLength(8)]
        public string CompanyId { get; set; }

        [ValidateNever]
        public Company Company { get; set; }
    }
}
