using AzureFunctionsApp.Models;
using AzureFunctionsApp.Repository.IRepository;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;



namespace AzureFunctionsApp.EmailFunctions
{
    public class SendEmailFunction
    {
        private readonly ISendEmailRepository _sendEmailRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SendEmailFunction(ISendEmailRepository sendEmailRepository, IUnitOfWork unitOfWork)
        {
            _sendEmailRepository = sendEmailRepository;
            _unitOfWork = unitOfWork;
        }

        [Function("SendEmailFunction")]
        public async Task Run([QueueTrigger("emailqueue", Connection = "AzureWebJobsStorage")] string message,
    FunctionContext context)
        {
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

                if (emailToSend.NotificationId != null)
                {
                    await UpdateNotificationStatus((int)emailToSend.NotificationId);
                }

                log.LogInformation($"Email sent to {emailToSend.EmailTo}");
            }
            catch (Exception ex)
            {
                log.LogError($"Error sending email: {ex.Message}");
            }
        }


        private async Task UpdateNotificationStatus(int notificationId)
        {
            NotificationStatus notificationStatusForUpdate;

            try
            {
                notificationStatusForUpdate = await _unitOfWork.NotificationStatus
                    .GetFirstOrDefaultAsync(x => x.Name == "Enviado");
            }
            catch (Exception)
            {
                notificationStatusForUpdate = await _unitOfWork.NotificationStatus
                    .GetFirstOrDefaultAsync(x => x.Name == "Envío fallido");
            }

            if (notificationStatusForUpdate == null)
            {
                throw new InvalidOperationException("Notification status 'Enviado' or 'Envío fallido' not found.");
            }

            var savedNotificationRecipients = await _unitOfWork.NotificationRecipient
                .GetAllAsync(x => x.NotificationId == notificationId);

            foreach (var recipient in savedNotificationRecipients)
            {
                recipient.NotificationStatusId = notificationStatusForUpdate.NotificationStatusId;
            }

            await _unitOfWork.SaveAsync();
        }

    }
}
