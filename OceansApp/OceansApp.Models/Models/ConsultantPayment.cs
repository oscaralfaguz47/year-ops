
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ConsultantPayment
    {
        [Required]
        [Key]
        public int ConsultantPaymentId { get; set; }
        [Required]
        public int ConsultantId { get; set; }
        [Required]
        public DateTime StartDatePeriod { get; set; }
        [Required]
        public DateTime EndDatePeriod { get; set; }
        [Required]
        public int ReferenceNumber { get; set; }
        [Required]
        public int PaymentMethodId { get; set; }
        [Required]
        public decimal PaymentAmount { get; set; }
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
        public int BankAccountId { get; set; }


        [ValidateNever]
        public ConsultantDetail ConsultantDetail { get; set; }
        [ValidateNever]
        public PaymentMethod PaymentMethod { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUserCreatedBy { get; set; }
        [ValidateNever]
        public ApplicationUser? ApplicationUserUpdatedBy { get; set; }
        [ValidateNever]
        public BankAccount BankAccount { get; set; }

    }
}
