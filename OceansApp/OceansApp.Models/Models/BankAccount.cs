
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class BankAccount
    {
        [Required]
        public int BankAccountId { get; set; }
        [MaxLength(20)]
        [Required]
        public string BankAccountCode { get; set; }
        [Required]
        [MaxLength(40)]
        public string BankAccountName { get; set; }
        [Required]
        [MaxLength(1)]
        public string IsActive { get; set; }

        [MaxLength(8)]
        public string CompanyId { get; set; }
    }
}
