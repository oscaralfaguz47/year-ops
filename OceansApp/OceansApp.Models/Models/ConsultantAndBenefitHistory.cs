
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ConsultantAndBenefitHistory
    {
        [Key]
        [Required]
        public int HistoryId { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        public string UserCreatedById { get; set; }
        [Required]
        public int ConsultantAndBenefitId { get; set; }
        [Required]
        public decimal OldValue { get; set; }
        [Required]
        public decimal NewValue { get; set; }
        public int? ReimbursedBenefitId { get; set; }

        [ValidateNever]
        public ConsultantAndBenefit ConsultantAndBenefit { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        [ValidateNever]
        public ConsultantReimbursedBenefit ConsultantReimbursedBenefit { get; set; }
    }
}
