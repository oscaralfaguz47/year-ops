
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class NotificationMedia
    {
        [Key]
        public int NotificationMediaId { get; set; }
        [Required]
        [MaxLength(25)]
        public string Name { get; set; }
    }
}
