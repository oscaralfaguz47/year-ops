
namespace OceansApp.Models.ViewModels.DocumentsCCSubtypes
{
    public class CreateUpdateDocumentSubtypeVM
    {
        public int? DocumentCCSubtypeId { get; set; }
        public string? DocumentTypeId { get; set; }
        public string? Description { get; set; }
        public string? CompanyId { get; set; }
        public int? CostCenterId { get; set; }
        public int? AccountingAccountId { get; set; }
    }
}
