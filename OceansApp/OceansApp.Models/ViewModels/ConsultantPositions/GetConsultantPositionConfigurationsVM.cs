
namespace OceansApp.Models.ViewModels.ConsultantPositions
{
    public class GetConsultantPositionConfigurationsVM
    {
        public int? Id { get; set; }
        public string CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int? CostCenterId { get; set; }
        public string? CostCenterName { get; set; }
        public int? AccountingAccountId { get; set; }
        public string? AccountingAccountName { get; set; }
        public int MovementTypeId { get; set; }
        public string? MovementTypeName { get; set; }
    }
}
