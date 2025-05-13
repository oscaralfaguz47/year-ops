
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.DocumentsCCSubtypes;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IDocumentCCSubtypeRepository : IRepository<DocumentCCSubtype>
    {
        Task<GetDocumentSubtypeVM> GetDocumentSubtypeByIdAsync(int docSubtypeId);
        Task<List<GetDocumentSubtypesListVM>> GetDocumentSubtypesListAsync();
        Task<MethodResponse> CreateDocumentSubType(CreateUpdateDocumentSubtypeVM docSubtypeData);
        Task<MethodResponse> UpdateDocumentSubtype(CreateUpdateDocumentSubtypeVM docSubtypeData);
    }
}
