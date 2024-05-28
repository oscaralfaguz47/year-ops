
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class TransactionType
    {
        [Required]
        [Key]
        public int TransactionTypeId { get; set; }
        [Required]
        [MaxLength(10)]
        public string Name { get; set; }
    }
}
