
namespace AzureFunctionsApp.Models
{
    public class SendEmailVM
    {
        public string EmailTo { get; set; }
        public List<string> EmailCcList { get; set; } = new List<string>();
        public string Subject { get; set; }
        public string? SharedEmailFrom { get; set; }
        public string Body { get; set; }
         public List<AttachmentVM> Attachments { get; set; } = new List<AttachmentVM>();
    }
}
