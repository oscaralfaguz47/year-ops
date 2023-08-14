
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OceansApp.Models.Models
{
    public class DocumentsCCNotification
    {
        [Required]
        public int DocumentCCId { get; set; }
        [ForeignKey("DocumentCCId")]
        [ValidateNever]
        public DocumentCC DocumentCC { get; set; }
        [Required]
        public int NotificationId { get; set; }
        [ForeignKey("NotificationId")]
        [ValidateNever]
        public Notification Notification { get; set; }
    }
}
