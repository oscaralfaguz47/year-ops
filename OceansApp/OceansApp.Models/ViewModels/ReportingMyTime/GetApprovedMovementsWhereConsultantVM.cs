
namespace OceansApp.Models.ViewModels.ReportingMyTime
{
    public class GetApprovedMovementsWhereConsultantVM
    {
        public int MovementId { get; set; }
        public int MovementTypeId { get; set; }
        public string MovementTypeName { get; set; }
        public decimal TotalQuantity { get; set; }
    }
}
