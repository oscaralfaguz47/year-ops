using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ConsultantPaymentDebitsCredits
    {
        [Required]
        [Key]
        public int ConsultantPaymentDebitsCreditsId { get; set; }
        [Required]
        public int ConsultantId { get; set; }
        [Required]
        public int AccountingAccountId { get; set; }
        [Required]
        public int CostCenterId { get; set; }
        [MaxLength(150)]
        public string? Detail { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public DateTime ActionDateWithinFortnight { get; set; }
        [Required]
        public int TransactionStatusId { get; set; }
        [Required]
        public int TransactionTypeId { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        public int ConsultantIdCreatedBy { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public int? ConsultantIdLastUpdatedBy { get; set; }


        [ValidateNever]
        public ConsultantDetail ConsultantDetail { get; set; }
        [ValidateNever]
        public AccountingAccount AccountingAccount { get; set; }
        [ValidateNever]
        public CostCenter CostCenter { get; set; }
        [ValidateNever]
        public TransactionStatus TransactionStatus { get; set; }
        [ValidateNever]
        public TransactionType TransactionType { get; set; }
        [ValidateNever]
        public ConsultantDetail ConsultantDetailCreatedBy { get; set; }
        [ValidateNever]
        public ConsultantDetail? ConsultantDetailUpdatedBy { get; set; }
    }
}
