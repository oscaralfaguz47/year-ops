using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;


namespace OceansApp.DataAccess.Repository
{
    public class DocumentCCSubtypeRepository : Repository<DocumentCCSubtype>, IDocumentCCSubtypeRepository
    {
        private ApplicationDbContext _db;
        public DocumentCCSubtypeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

    }
}
