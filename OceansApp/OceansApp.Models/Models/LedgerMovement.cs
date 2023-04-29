using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class LedgerMovement
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(10)]
        public string IdSeat { get; set; }
        [Required]
        public int Consecutive { get; set; }
        [Required]
        [MaxLength(25)]
        public string IdCostCenter { get; set; }
        [ForeignKey("IdCostCenter")]
        [ValidateNever]
        public CostCenter CostCenter { get; set; }
        [Required]
        [MaxLength(25)]
        public string IdAccountingAccount { get; set; }
        [ForeignKey("IdAccountingAccount")]
        [ValidateNever]
        public AccountingAccount AccountingAccount { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public Decimal LocalDebit { get; set; }
        [Required]
        public Decimal LocalCredit { get; set; }
        [Required]
        [MaxLength(1)]
        public string AccountingType { get; set; }
        [Required]
        public DateTime RecordDate { get; set; }

    }
}
