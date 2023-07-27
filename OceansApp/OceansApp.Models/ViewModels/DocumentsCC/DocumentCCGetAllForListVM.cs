
using OceansApp.Models.ViewModels.Components;

namespace OceansApp.Models.ViewModels.DocumentsCC
{
    public class DocumentCCGetAllForListVM
    {
        public DocumentCCFiltersGetAllVM Filters { get; set; }
        public Pagination Pagination { get; set; }
        public List<DocumentCCGetAllWithFiltersVM>? DocumentsCCList { get; set; }
        public List<SelectVM>? ClientList { get; set; }
        public List<SelectVM>? DocumentTypeList { get; set; }
    }
}
