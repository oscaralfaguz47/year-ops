

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class NotificationRecipient
    {
        [Key]
        public int RecipientId { get; set; }
        [Required]
        [MaxLength(150)]
        public string RecipientMediaInfo { get; set; }
        [Required]
        public int NotificationId { get; set; }
        [ForeignKey("NotificationId")]
        [ValidateNever]
        public Notification Notification { get; set; }
        [Required]
        public int NotificationMediaId { get; set; }
        [ForeignKey("NotificationMediaId")]
        [ValidateNever]
        public NotificationMedia NotificationMedia { get; set; }
        [Required]
        public int NotificationStatusId { get; set; }
        [ForeignKey("NotificationStatusId")]
        [ValidateNever]
        public NotificationStatus NotificationStatus { get; set; }
        [MaxLength(450)]
        public string? RecipientUserId { get; set; }
        [ForeignKey("RecipientUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
