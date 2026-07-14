using System.Globalization;
using System.Text;
using Azure.Storage.Queues;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using OceansApp.Models.ViewModels;
using OceansApp.Utility.NotificationTemplates;

namespace OceansAppWeb.Helpers
{
    /// <summary>
    /// Shared "new submission to review" internal notification. Extracted from the two identical
    /// copies that lived in ReportingMyTimeController (consultant self-report) and PaymentSheetsController
    /// (admin on-behalf upload) so the email-template + queue-send logic exists once.
    /// </summary>
    public static class SubmissionNotifications
    {
        public static async Task SendNewSubmissionToReview(Lazy<QueueClient> queueClient, IConfiguration config,
            TelemetryClient telemetryClient, string baseUrl, string consultantName, string projectName,
            DateTime startDate, DateTime endDate)
        {
            var emailTemplates = new EmailTemplates();
            string startDateFormated = startDate.ToString("MMM d", CultureInfo.InvariantCulture);
            string endDateFormated = endDate.ToString("MMM d", CultureInfo.InvariantCulture);
            string periodString = $"{startDateFormated} - {endDateFormated}";

            var createNotificationBody = emailTemplates.SubmissionHoursNotificationBody(baseUrl,
                (consultantName ?? string.Empty).Trim(), periodString, (projectName ?? string.Empty).Trim());
            var templateEmail = emailTemplates.EmailTemplate("NEW SUBMISSION TO REVIEW", createNotificationBody);

            SendEmailVM emailToSend = new()
            {
                Subject = "New Submission to Review - Ripple by Oceans",
                SharedEmailFrom = config["SharedMailboxEmailRippleApp"],
                EmailTo = config["InternalEmailENV"],
                Body = templateEmail
            };

            var messageContent = JsonConvert.SerializeObject(emailToSend);
            try
            {
                await queueClient.Value.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(messageContent)));
            }
            catch (Exception)
            {
                telemetryClient.TrackTrace($"Fail sending email to: {config["InternalEmailENV"]}, the connection with the function app failed");
            }
        }
    }
}
