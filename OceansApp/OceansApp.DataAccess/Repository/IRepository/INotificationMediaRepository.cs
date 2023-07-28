using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface INotificationMediaRepository : IRepository<NotificationMedia> 
    {
        void Update(NotificationMedia obj);

    }
}
