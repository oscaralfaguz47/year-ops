
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ConsultantAndBenefit
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public int ConsultantId { get; set; }
        [Required]
        public int BenefitId { get; set; }
        [Required]
        public decimal BalanceAmount { get; set; }


        [ValidateNever]
        public ConsultantDetail ConsultantDetail { get; set; }
        [ValidateNever]
        public ConsultantBenefit ConsultantBenefit { get; set; }
    }
}
