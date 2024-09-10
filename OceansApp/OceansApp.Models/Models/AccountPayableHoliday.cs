
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class AccountPayableHoliday
    {
        [Required]
        [Key]
        public int AccountPayableHolidayId { get; set; }
        [Required]
        public int AccountPayableId { get; set; }
        [MaxLength(249)]
        [Required]
        public string Reference { get; set; }
        [Required]
        public decimal TotalAmount { get; set; }

        [ValidateNever]
        public AccountPayable AccountPayable { get; set; }
    }
}
