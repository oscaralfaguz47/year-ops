
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class AccountPayable
    {
        [Key]
        [Required]
        public int AccountPayableId { get; set; }
        [Required]
        public int ConsultantId { get; set; }
        [Required]
        public DateTime StartDatePeriod { get; set; }
        [Required]
        public DateTime EndDatePeriod { get; set; }
        [Required]
        public DateTime AccountingDate { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public decimal BalanceAmount { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        [MaxLength(450)]
        public string UserCreatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        [MaxLength(450)]
        public string? UserLastUpdatedBy { get; set; }
        [MaxLength(8)]
        public string CompanyId { get; set; }
        [Required]
        public int TransactionStatusId { get; set; }
        [Required]
        public bool Voided { get; set; }


        [ValidateNever]
        public ConsultantDetail ConsultantDetail { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUserCreatedBy { get; set; }
        [ValidateNever]
        public ApplicationUser? ApplicationUserUpdatedBy { get; set; }
        [ValidateNever]
        public TransactionStatus TransactionStatus { get; set; }
        [ValidateNever]
        public Company Company { get; set; }
    }
}
