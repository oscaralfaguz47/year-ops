
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class NotificationType
    {
        [Key]
        public int NotificationTypeId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
