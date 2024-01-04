using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Notifications;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface INotificationRepository : IRepository<Notification> 
    {
        void Update(Notification obj);
        int SaveNotification(string from, string subject, string body, string remitent,
            string sentByUser, List<SaveNotificationRecipientVM> recipients, int notificationStatus);

    }
}
