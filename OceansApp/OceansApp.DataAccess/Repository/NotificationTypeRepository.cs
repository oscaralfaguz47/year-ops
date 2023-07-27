using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class NotificationTypeRepository : Repository<NotificationType>, INotificationTypeRepository
    {
        private ApplicationDbContext _db;
        public NotificationTypeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
       
        public void Update(NotificationType obj)
        {
            _db.NOTIFICATION_TYPES.Update(obj);
        }

    }
}
