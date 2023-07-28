using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }
        public int NotificationTypeId { get; set; }
        [ForeignKey("NotificationTypeId")]
        [ValidateNever]
        public NotificationType NotificationType { get; set; }
        [Required]
        public string Body { get; set; }
        [Required]
        [MaxLength(100)]
        public string Subject { get; set; }
        [Required]
        [MaxLength(150)]
        public string Remitent { get; set; }
        [Required]
        public DateTime SentDate { get; set; }
    }
}
