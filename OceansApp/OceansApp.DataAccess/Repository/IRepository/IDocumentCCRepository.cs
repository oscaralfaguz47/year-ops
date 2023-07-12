
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IDocumentCCRepository : IRepository<DocumentCC> 
    {
        void Update(DocumentCC obj);
        public bool UpdateIfExistAddIfNot(DocumentCC obj);
    }
}
