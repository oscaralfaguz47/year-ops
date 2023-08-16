
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.DocumentsCC;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IDocumentCCRepository : IRepository<DocumentCC> 
    {
        IEnumerable<SelectVM> GetDocumentsTypeWhereDocumentsExist();
        Task<(List<DocumentCCGetAllWithFiltersVM> documentsCC, int totalCount)> GetAllDocumentsCCWithFiltersAsync(DocumentCCGetAllForListVM filtersAndPagination);
        Task<List<DocumentCCGetNotificationsHistoryVM>> GetNotificationsHistoryByDocumentIdAsync(int documentId);
        void Update(DocumentCC obj);
        public bool UpdateIfExistAddIfNot(DocumentCC obj);
    }
}
