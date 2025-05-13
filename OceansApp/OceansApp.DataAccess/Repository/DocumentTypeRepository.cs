using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;


namespace OceansApp.DataAccess.Repository
{
    public class DocumentTypeRepository : Repository<DocumentType>, IDocumentTypeRepository
    {
        private ApplicationDbContext _db;
        public DocumentTypeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

    }
}
