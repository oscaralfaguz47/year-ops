using Azure.Security.KeyVault.Secrets;
using AzureFunctionsApp.Models;
using AzureFunctionsApp.Repository.IRepository;
using MailKit.Security;
using Microsoft.ApplicationInsights;
using MimeKit;

namespace AzureFunctionsApp.Repository
{
    public class SendEmailRepository : ISendEmailRepository
    {
        private readonly SecretClient _secretClient;
        private readonly TelemetryClient _telemetryClient;
        private string _senderName;
        private string _emailFrom;
        private string _emailFromPassword;
        private bool _isInitialized = false;

        public SendEmailRepository(SecretClient secretClient, TelemetryClient telemetryClient)
        {
            _secretClient = secretClient;
            _telemetryClient = telemetryClient;
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                _senderName = (await _secretClient.GetSecretAsync("internalEmailSenderName")).Value.Value;
                _emailFrom = (await _secretClient.GetSecretAsync("internalEmail")).Value.Value;
                _emailFromPassword = (await _secretClient.GetSecretAsync("internalEmailPass")).Value.Value;
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                throw new InvalidOperationException("Error loading secrets from Key Vault", ex);
            }
        }


        public async Task<string?> SendEmail(SendEmailVM emailModel)
        {
            await InitializeAsync();

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

            var bodyBuilder = new BodyBuilder { HtmlBody = emailModel.Body };

            foreach (var attachment in emailModel.Attachments)
            {
                bodyBuilder.Attachments.Add(attachment.FileName, attachment.FileContent, ContentType.Parse("application/octet-stream"));
            }

            message.Body = bodyBuilder.ToMessageBody();

            int retryCount = 3;
            for (int i = 0; i < retryCount; i++)
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    try
                    {
                        var smtpHost = "smtp.office365.com";
                        var smtpPort = 587;

                        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                        await client.AuthenticateAsync(_emailFrom, _emailFromPassword);
                        await client.SendAsync(message);
                        _telemetryClient.TrackTrace($"Email sent to {emailModel.EmailTo}");
                    }
                    catch (Exception ex)
                    {
                        _telemetryClient.TrackException(ex);
                        if (i == retryCount - 1)
                        {
                            return $"Failed to send email after {retryCount} attempts: {ex.Message}";
                        }
                        await Task.Delay(2000);
                    }
                    finally
                    {
                        if (client.IsConnected)
                        {
                            await client.DisconnectAsync(true);
                        }
                        client.Dispose();
                    }
                }

            }

            return null;
        }
    }

}
