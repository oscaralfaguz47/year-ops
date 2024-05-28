
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class CostCenterAccountingAccount
    {
        [Key]
        [Required]
        public int CostCenterAccountingAccountId { get; set; }
        [Required]
        public int CostCenterId { get; set; }
        [Required]
        public int AccountingAccountId { get; set; }
        [Required]
        [MaxLength(1)]
        public string Status { get; set; }
        [Required]
        public DateTime CreateDate { get; set; }
        [MaxLength(8)]
        [Required]
        public string CompanyId { get; set; }

        [ValidateNever]
        public CostCenter CostCenter { get; set; }
        [ValidateNever]
        public AccountingAccount AccountingAccount { get; set; }
    }
}
