using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Utility.Email;

namespace OceansApp.DataAccess.Repository
{
    public class SendEmailRepository : ISendEmailRepository
    {
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;

        public SendEmailRepository(IEmailSender emailSender, IConfiguration config)
        {
            _emailSender = emailSender;
            _config = config;
        }

        public async Task<string?> SendEmail(SendEmailVM emailModel)
        {
            try
            {
                var message = new MimeMessage();
                if (emailModel.SharedEmailFrom != null)
                {
                    message.From.Add(new MailboxAddress(_config["internalEmailSenderName"], emailModel.SharedEmailFrom));
                }
                else
                {
                    message.From.Add(new MailboxAddress(_config["internalEmailSenderName"], _config["internalEmail"]));
                }
                message.To.Add(new MailboxAddress("", emailModel.EmailTo));
                message.Subject = emailModel.Subject;
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = emailModel.Body;
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);

                    // Authentication with the credentials of the user that has delegated access to the Shared Mailbox
                    await client.AuthenticateAsync(_config["internalEmail"], _config["pass"]);

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
