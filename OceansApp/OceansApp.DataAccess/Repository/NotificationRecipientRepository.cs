using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class NotificationRecipientRepository : Repository<NotificationRecipient>, INotificationRecipientRepository
    {
        private ApplicationDbContext _db;
        public NotificationRecipientRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        
        public void Update(NotificationRecipient obj)
        {
            _db.NOTIFICATION_RECIPIENTS.Update(obj);
        }

    }
}
