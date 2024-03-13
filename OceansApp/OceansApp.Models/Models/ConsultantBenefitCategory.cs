
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ConsultantBenefitCategory
    {
        [Required]
        [Key]
        public int BenefitCategoryId { get; set; }
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }
        [Required]
        public int BenefitId { get; set; }

        [ValidateNever]
        public ConsultantBenefit ConsultantBenefit { get; set; }
    }
}
