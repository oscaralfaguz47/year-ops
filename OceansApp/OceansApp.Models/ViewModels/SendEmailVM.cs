
using System.Net.Mail;

namespace OceansApp.Models.ViewModels
{
    public class SendEmailVM
    {
        public string EmailTo { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string? SharedEmailFrom { get; set; }
        public List<string>? EmailCcList { get; set; } = new List<string>();
        public int? NotificationId { get; set; }
        public List<AttachmentVM> Attachments { get; set; } = new List<AttachmentVM>();
    }
}
