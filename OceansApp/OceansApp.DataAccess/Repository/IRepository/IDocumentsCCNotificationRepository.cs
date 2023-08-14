using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IDocumentsCCNotificationRepository : IRepository<DocumentsCCNotification> 
    {
        void Update(DocumentsCCNotification obj);

    }
}
