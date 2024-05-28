
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class PaymentMethod
    {
        [Required]
        [Key]
        public int PaymentMethodId { get; set; }
        [Required]
        [MaxLength(70)]
        public string Name { get; set; }
        [Required]
        [MaxLength(8)]
        public string CompanyId { get; set; }
    }
}
