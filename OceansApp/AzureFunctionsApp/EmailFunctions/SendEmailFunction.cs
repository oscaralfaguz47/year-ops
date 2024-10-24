using AzureFunctionsApp.Models;
using AzureFunctionsApp.Repository.IRepository;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace AzureFunctionsApp.EmailFunctions
{
    public class SendEmailFunction
    {
        private readonly ISendEmailRepository _sendEmailRepository;
        private readonly TelemetryClient _telemetryClient;

        public SendEmailFunction(ISendEmailRepository sendEmailRepository, TelemetryClient telemetryClient)
        {
            _sendEmailRepository = sendEmailRepository;
            _telemetryClient = telemetryClient;
        }

        [Function("SendEmailFunction")]
        public async Task Run([QueueTrigger("emailqueue", Connection = "AzureWebJobsStorage")] string message,
    FunctionContext context)
        {
            _telemetryClient.TrackTrace($"My function is working");
            var log = context.GetLogger("SendEmailFunction");
            try
            {
                var emailToSend = JsonConvert.DeserializeObject<SendEmailVM>(message);
                if (emailToSend == null)
                {
                    log.LogError("The deserialized object is null");
                    return;
                }

                await _sendEmailRepository.SendEmail(emailToSend);

                log.LogInformation($"Email sent to {emailToSend.EmailTo}");
            }
            catch (Exception ex)
            {
                log.LogError($"Error sending email: {ex.Message}");
            }
        }

    }
}
