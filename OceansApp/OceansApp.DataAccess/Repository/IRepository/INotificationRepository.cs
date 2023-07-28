using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface INotificationRepository : IRepository<Notification> 
    {
        void Update(Notification obj);

    }
}
