
using OceansApp.Models.ViewModels.Components;

namespace OceansApp.Models.ViewModels.DocumentsCC
{
    public class GetSubtypesListAndDocTypeConsecutiveNumberVM
    {
        public int DocConsecutiveNumber { get; set; }
        public List<SelectVM> SubtypesList { get; set; }
    }
}
