
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class ProviderEvent
    {
        [Key]
        public int ProviderEventId { get; set; }
        [MaxLength(30)]
        [Required]
        public string Name { get; set; }
    }
}
