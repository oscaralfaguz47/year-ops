using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface INotificationTypeRepository : IRepository<NotificationType> 
    {
        void Update(NotificationType obj);

    }
}
