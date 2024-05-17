using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Notifications;

namespace OceansApp.DataAccess.Repository
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        private ApplicationDbContext _db;
        public NotificationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
      
        public void Update(Notification obj)
        {
            _db.NOTIFICATIONS.Update(obj);
        }

        public int SaveNotification(string from, string subject, string body, string remitent, 
            string sentByUser, List<SaveNotificationRecipientVM> recipients, int notificationStatus)
        {
            var notificationType = _db.NOTIFICATION_TYPES.FirstOrDefault(x => x.Name == from);
            var notification = new Notification()
            {
                NotificationTypeId = notificationType.NotificationTypeId,
                Body = body,
                Subject = subject,
                Remitent = remitent,
                SentDate = DateTime.UtcNow,
                SentByUser = sentByUser
            };
            _db.NOTIFICATIONS.Add(notification);
            _db.SaveChanges();

            foreach (var recipient in recipients)
            {
                var recipientUser = _db.Users.FirstOrDefault(x => x.Email == recipient.RecipientMediaInfo);
                var recipientUserIdCC = recipientUser?.Id;
                var notificationMedia = _db.NOTIFICATION_MEDIA.FirstOrDefault(x => x.Name == recipient.NotificationMedia);
                var notificationRecipient = new NotificationRecipient()
                {
                    RecipientMediaInfo = recipient.RecipientMediaInfo,
                    NotificationId = notification.NotificationId,
                    NotificationMediaId = notificationMedia.NotificationMediaId,
                    NotificationStatusId = notificationStatus,
                    RecipientUserId = recipientUserIdCC
                };
                _db.NOTIFICATION_RECIPIENTS.Add(notificationRecipient);
            }
            _db.SaveChanges();
            return notification.NotificationId;
        }

    }
}
