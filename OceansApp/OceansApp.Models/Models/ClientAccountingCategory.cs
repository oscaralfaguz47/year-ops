
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ClientAccountingCategory
    {
        [Required]
        [Key]
        public int ClientAccountingCategoryId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Description { get; set; }
        public int CostCenterIdSalesReturn { get; set; }
        public int AccountingAccountIdSalesReturn { get; set; }
        public int CostCenterIdSalesDiscounts { get; set; }
        public int AccountingAccountIdSalesDiscounts { get; set; }
        [Required]
        [MaxLength(8)]
        public string CompanyId { get; set; }



        [ValidateNever]
        public CostCenter CostCenterSalesReturn { get; set; }
        [ValidateNever]
        public AccountingAccount AccountingAccountSalesReturn { get; set; }
        [ValidateNever]
        public int CostCenterSalesDiscount { get; set; }
        [ValidateNever]
        public AccountingAccount AccountingAccountSalesDiscount { get; set; }
        [ValidateNever]
        public Company Company { get; set; }
    }
}
