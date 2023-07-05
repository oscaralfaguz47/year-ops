
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class CalculatorAccountingAccountToIgnore
    {

        [Key]
        public int AccountingAccountId { get; set; }
        [ForeignKey("AccountingAccountId")]
        [ValidateNever]
        public AccountingAccount AccountingAccount { get; set; }
        public DateTime CreationDate { get; set; }

    }
}
