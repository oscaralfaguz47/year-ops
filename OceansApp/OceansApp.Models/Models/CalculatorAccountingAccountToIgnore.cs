
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class CalculatorAccountingAccountToIgnore
    {

        [Key]
        [MaxLength(25)]
        public string? IdAccountingAccount { get; set; }
        [Required]
        [MaxLength(25)]
        public string ExpenseType { get; set; }

    }
}
