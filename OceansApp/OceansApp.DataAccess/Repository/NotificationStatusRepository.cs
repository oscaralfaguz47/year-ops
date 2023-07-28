using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class NotificationStatusRepository : Repository<NotificationStatus>, INotificationStatusRepository
    {
        private ApplicationDbContext _db;
        public NotificationStatusRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(NotificationStatus obj)
        {
            _db.NOTIFICATION_STATUS.Update(obj);
        }

    }
}
