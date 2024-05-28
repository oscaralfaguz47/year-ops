
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class TransactionStatus
    {
        [Required]
        [Key]
        public int TransactionStatusId { get; set; }
        [MaxLength(80)]
        [Required]
        public string Name { get; set; }
    }
}
