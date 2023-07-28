using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace OceansApp.Utility.Email
{
    public class EmailSender : IEmailSender
    {
        public IConfiguration _config { get; }
        public EmailSender(IConfiguration config)
        {
            _config = config;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                MailMessage oMailMessage = new MailMessage(_config["internalEmail1"], email, subject, htmlMessage);

                oMailMessage.IsBodyHtml = true;

                SmtpClient oSmtpClient = new SmtpClient("smtp.office365.com");
                oSmtpClient.EnableSsl = true;
                oSmtpClient.UseDefaultCredentials = false;
                oSmtpClient.Host = "smtp.office365.com";
                oSmtpClient.Port = 587;
                oSmtpClient.Credentials = new System.Net.NetworkCredential(_config["internalEmail1"], _config["pass"]);

                oSmtpClient.Send(oMailMessage);
                oSmtpClient.Dispose();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                // TODO: handle exception
                throw new InvalidOperationException(ex.Message);
            }

        }
    }
}
