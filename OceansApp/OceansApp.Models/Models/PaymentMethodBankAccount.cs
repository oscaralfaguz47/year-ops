
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class PaymentMethodBankAccount
    {
        [Required]
        public int PaymentMethodId { get; set; }
        [Required]
        public int BankAccountId { get; set; }
        [Required]
        public bool IsDefault { get; set; }

        [ValidateNever]
        public PaymentMethod PaymentMethod { get; set; }
        [ValidateNever]
        public BankAccount BankAccount { get; set; }
    }
}
