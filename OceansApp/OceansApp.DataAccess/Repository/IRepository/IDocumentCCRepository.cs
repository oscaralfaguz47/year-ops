
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.DocumentsCC;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IDocumentCCRepository : IRepository<DocumentCC> 
    {
        Task<IEnumerable<SelectVM>> GetDocumentsTypeWhereDocumentsExistAsync();
        Task<List<DocumentCCGetExpiredDocsVM>> GetAllExpiredDocsWithDaysExpiredFiltersAsync();
        Task<List<DocumentCCGetExpiredDocsVM>> GetAllExpiredPendingDocsAsync();
        Task<(List<DocumentCCGetAllWithFiltersVM> documentsCC, int totalCount)> GetAllDocumentsCCWithFiltersAsync(DocumentCCPaginationFiltersVM filtersAndPagination);
        Task<List<DocumentCCGetNotificationsHistoryVM>> GetNotificationsHistoryByDocumentIdAsync(int documentId);
        void Update(DocumentCC obj);
        Task<bool> UpdateIfExistAddIfNot(DocumentCC obj);
    }
}
