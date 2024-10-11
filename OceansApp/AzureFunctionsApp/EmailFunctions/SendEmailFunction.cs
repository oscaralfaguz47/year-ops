using AzureFunctionsApp.Models;
using AzureFunctionsApp.Repository.IRepository;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace AzureFunctionsApp.EmailFunctions
{
    public class SendEmailFunction
    {
        private readonly ISendEmailRepository _sendEmailRepository;

        public SendEmailFunction(ISendEmailRepository sendEmailRepository)
        {
            _sendEmailRepository = sendEmailRepository;
        }

        [FunctionName("SendEmailFunction")]
        public async Task Run(
            [QueueTrigger("emailqueue", Connection = "AzureWebJobsStorage")] string message,
            ILogger log)
        {
            try
            {
                var emailData = JsonConvert.DeserializeObject<SendEmailVM>(message);
                await _sendEmailRepository.SendEmail(emailData);
                log.LogInformation($"Email sent to {emailData.EmailTo}");
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to send email: {ex.Message}");
            }
        }
    }
}
