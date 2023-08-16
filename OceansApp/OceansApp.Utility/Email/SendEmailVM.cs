
namespace OceansApp.Utility.Email
{
    public class SendEmailVM
    {
        public string EmailTo { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string? SharedEmailFrom { get; set; }
        public List<string>? EmailCcList { get; set; }
    }
}
