
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ProductClientCompanyAccountingConfigForBilling
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int ClientId { get; set; }
        [MaxLength(8)]
        [Required]
        public required string CompanyId { get; set; }
        public int? MovementTypeId { get; set; }
        [Required]
        public int CostCenterIdSales { get; set; }

        [Required]
        public int CostCenterIdSalesDiscount { get; set; }
        [Required]
        public int CostCenterIdSalesReturn { get; set; }
        [Required]
        public int AccountingAccountIdSales { get; set; }
        [Required]
        public int AccountingAccountIdSalesDiscount { get; set; }
        [Required]
        public int AccountingAccountIdSalesReturn { get; set; }
        [Required]
        public decimal TaxPercentage { get; set; }
        public int? CostCenterIdTaxPercentage { get; set; }
        public int? AccountingAccountIdTaxPercentage { get; set; }

        [ValidateNever]
        public Product Product { get; set; }
        [ValidateNever]
        public Client Client { get; set; }
        [ValidateNever]
        public ReportingMyTimeMovementType? ReportingMyTimeMovementType { get; set; }
        [ValidateNever]
        public Company Company { get; set; }
        [ValidateNever]
        public CostCenter CostCenterSales { get; set; }
        [ValidateNever]
        public CostCenter CostCenterSalesDiscount { get; set; }
        [ValidateNever]
        public CostCenter CostCenterSalesReturn { get; set; }
        [ValidateNever]
        public AccountingAccount AccountingAccountSales { get; set; }
        [ValidateNever]
        public AccountingAccount AccountingAccountSalesDiscount { get; set; }
        [ValidateNever]
        public AccountingAccount AccountingAccountSalesReturn { get; set; }
        [ValidateNever]
        public CostCenter? CostCenterTaxPercentage { get; set; }
        [ValidateNever]
        public AccountingAccount? AccountingAccountTaxPercentage { get; set; }
    }
}
