using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class DocumentsCCNotificationRepository : Repository<DocumentsCCNotification>, IDocumentsCCNotificationRepository
    {
        private ApplicationDbContext _db;
        public DocumentsCCNotificationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
   
        public void Update(DocumentsCCNotification obj)
        {
            _db.DOCUMENTS_CC_NOTIFICATIONS.Update(obj);
        }

    }
}
