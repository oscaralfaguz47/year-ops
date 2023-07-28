using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface INotificationStatusRepository : IRepository<NotificationStatus> 
    {
        void Update(NotificationStatus obj);

    }
}
