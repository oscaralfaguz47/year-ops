
using System.ComponentModel.DataAnnotations;

namespace OceansApp.Models.Models
{
    public class NotificationStatus
    {
        [Key]
        public int NotificationStatusId { get; set; }
        [Required]
        [MaxLength(25)]
        public string Name { get; set; }
    }
}
