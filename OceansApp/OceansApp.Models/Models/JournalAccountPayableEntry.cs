
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class JournalAccountPayableEntry
    {
        [Key]
        [Required]
        public int JournalEntryId { get; set; }
        [Required]
        public int CostCenterId { get; set; }
        [Required]
        public int AccountingAccountId { get; set; }
        [Required]
        [MaxLength(249)]
        public string Reference { get; set; }
        [Required]
        public decimal Debit { get; set; }
        [Required]
        public decimal Credit { get; set; }
        [Required]
        public int AccountPayableId { get; set; }
        [Required]
        public int JournalId { get; set; }


        [ValidateNever]
        public CostCenter CostCenter { get; set; }
        [ValidateNever]
        public AccountingAccount AccountingAccount { get; set; }
        [ValidateNever]
        public AccountPayable AccountPayable { get; set; }
        [ValidateNever]
        public JournalAccountPayable JournalAccountPayable { get; set; }
    }
}
