using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace OceansApp.Utility.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await SendEmailAsync(email, subject, htmlMessage, null);
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage, string? sharedMailboxFrom)
        {
            try
            {
                var message = new MimeMessage();
                if (sharedMailboxFrom != null)
                {
                    message.From.Add(new MailboxAddress(_config["internalEmailSenderName"], sharedMailboxFrom));
                }
                else
                {
                    message.From.Add(new MailboxAddress(_config["internalEmailSenderName"], _config["internalEmail"]));
                }
               
                message.To.Add(new MailboxAddress("", email)); 
                message.Subject = subject;
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = htmlMessage;
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);

                    // Autenticación con las credenciales del usuario que tiene acceso delegado al Shared Mailbox
                    await client.AuthenticateAsync(_config["internalEmail"], _config["pass"]);

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones (puedes personalizarlo según tus necesidades)
                throw new InvalidOperationException("Error al enviar el correo electrónico.", ex);
            }
        }
    }
}
