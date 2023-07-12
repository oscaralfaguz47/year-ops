using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class DocumentCCRepository : Repository<DocumentCC>, IDocumentCCRepository
    {
        private ApplicationDbContext _db;
        public DocumentCCRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(DocumentCC obj)
        {
            _db.DOCUMENTS_CC.Update(obj);
        }

        public bool UpdateIfExistAddIfNot(DocumentCC obj)
        {
            var existingDoc = GetFirstOrDefault(u => u.DocumentNumber == obj.DocumentNumber && u.DocumentType == obj.DocumentType && u.CompanyId == obj.CompanyId);
            if (existingDoc == null)
            {
                _db.DOCUMENTS_CC.Add(obj);
                _db.SaveChanges();
                return true;
            }
            else
            {
                if (existingDoc.DateLastUpdate != obj.DateLastUpdate)
                {
                    existingDoc.DocumentNumber = obj.DocumentNumber;
                    existingDoc.DocumentType = obj.DocumentType;
                    existingDoc.ApplicationDescription = obj.ApplicationDescription;
                    existingDoc.DocumentDate = obj.DocumentDate;
                    existingDoc.DocumentAmount = obj.DocumentAmount;
                    existingDoc.BalanceAmount = obj.BalanceAmount;
                    existingDoc.Canceled = obj.Canceled;
                    existingDoc.IdSeat = obj.IdSeat;
                    existingDoc.DateLastUpdate = obj.DateLastUpdate;
                    existingDoc.CreationDate = obj.CreationDate;
                    existingDoc.CompanyId = obj.CompanyId;
                    return true;
                }
                return false;
            }
        }

    }
}
