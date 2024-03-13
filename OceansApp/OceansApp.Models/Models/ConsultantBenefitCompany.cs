
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ConsultantBenefitCompany
    {
        [Required]
        [Key]
        public int ConsultantaBenefitCompanyId { get; set; }
        [Required]
        [MaxLength(8)]
        public string CompanyId { get; set; }
        [Required]
        public int CostCenterId { get; set; }
        [Required]
        public int AccountingAccountId { get; set; }

        [ValidateNever]
        public CostCenter CostCenter { get; set; }
        [ValidateNever]
        public AccountingAccount AccountingAccount { get; set; }
    }
}
