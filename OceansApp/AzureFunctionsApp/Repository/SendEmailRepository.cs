using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureFunctionsApp.Models;
using AzureFunctionsApp.Repository.IRepository;
using MailKit.Security;
using MimeKit;

namespace AzureFunctionsApp.Repository
{
    public class SendEmailRepository : ISendEmailRepository
    {
        private readonly SecretClient _secretClient;
        private string _senderName;
        private string _emailFrom;
        private string _emailFromPassword;

        public SendEmailRepository()
        {
            var vaultUri = Environment.GetEnvironmentVariable("VaultUri");
            _secretClient = new SecretClient(new Uri(vaultUri), new DefaultAzureCredential());
            InitializeSecrets().Wait();
        }

        private async Task InitializeSecrets()
        {
            _senderName = (await _secretClient.GetSecretAsync("internalEmailSenderName")).Value.Value;
            _emailFrom = (await _secretClient.GetSecretAsync("internalEmail")).Value.Value;
            _emailFromPassword = (await _secretClient.GetSecretAsync("internalEmailPass")).Value.Value;
        }

        public async Task<string?> SendEmail(SendEmailVM emailModel)
        {
            var message = new MimeMessage();
            var fromAddress = emailModel.SharedEmailFrom ?? _emailFrom;

            message.From.Add(new MailboxAddress(_senderName, fromAddress));
            message.To.Add(new MailboxAddress("", emailModel.EmailTo));

            if (emailModel.EmailCcList != null)
            {
                foreach (var ccEmail in emailModel.EmailCcList)
                {
                    message.Cc.Add(new MailboxAddress("", ccEmail));
                }
            }

            message.Subject = emailModel.Subject;
            message.Body = new BodyBuilder { HtmlBody = emailModel.Body }.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                var smtpHost = "smtp.office365.com"; // Puedes mover esto a Key Vault si necesitas cambiarlo.
                var smtpPort = 587; // Igualmente, puedes moverlo a Key Vault para flexibilidad.

                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailFrom, _emailFromPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            return null;
        }
    }
}
