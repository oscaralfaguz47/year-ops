
namespace OceansApp.Models.ViewModels.ConsultantPositions
{
    public class ConsultantPositionsGetAllWithFiltersVM
    {
        public int? Id { get; set; }
        public int ConsultantPositionId { get; set; }
        public string PositionName { get; set; }
        public bool IsPositionAdministrative { get; set; }
        public string? MovementTypeName { get; set; }
        public string? CompanyId { get; set; }
        public string? CostCenterCode { get; set; }
        public string? CostCenterName { get; set; }
        public string? AccountingAccountCode { get; set; }
        public string? AccountingAccountName { get; set; }
    }
}
