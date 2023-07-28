using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface INotificationRecipientRepository : IRepository<NotificationRecipient> 
    {
        void Update(NotificationRecipient obj);

    }
}
