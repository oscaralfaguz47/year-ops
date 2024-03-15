
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ConsultantReimbursedBenefit
    {
        [Key]
        [Required]
        public int ReimbursedBenefitId { get; set; }
        [Required]
        public int BenefitId { get; set; }
        [MaxLength(150)]
        public string? Detail { get; set; }

        [Required]
        public int ConsultantId { get; set; }
        [Required]
        public decimal AmountReimbursed { get; set; }
        [Required]
        public DateTime DateToBeReimbursed { get; set; }
        [Required]
        public bool BenefitPaid { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        public int ConsultantIdCreatedBy { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public int? ConsultantIdLastUpdatedBy { get; set; }
        [Required]
        public int BenefitCategoryId { get; set; }


        [ValidateNever]
        public ConsultantBenefit ConsultantBenefit { get; set; }
        [ValidateNever]
        public ConsultantDetail ConsultantDetailBenefit { get; set; }
        [ValidateNever]
        public ConsultantDetail ConsultantDetailCreatedBy { get; set; }
        [ValidateNever]
        public ConsultantDetail? ConsultantDetailUpdatedBy { get; set; }
        [ValidateNever]
        public ConsultantBenefitCategory ConsultantBenefitCategory { get; set; }
    }
}
