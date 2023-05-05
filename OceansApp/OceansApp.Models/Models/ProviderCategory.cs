
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ProviderCategory
    {
        [Key]
        [MaxLength(8)]
        [Required]
        public string IdProviderCategory { get; set; }
        [Required]
        [MaxLength(40)]
        public string Description { get; set; }
        [Required]
        public DateTime CreateDate { get; set; }
    }
}
