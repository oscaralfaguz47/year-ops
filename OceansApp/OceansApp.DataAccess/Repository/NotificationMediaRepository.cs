using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class NotificationMediaRepository : Repository<NotificationMedia>, INotificationMediaRepository
    {
        private ApplicationDbContext _db;
        public NotificationMediaRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(NotificationMedia obj)
        {
            _db.NOTIFICATION_MEDIA.Update(obj);
        }

    }
}
