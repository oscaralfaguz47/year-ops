
namespace OceansApp.Models.ViewModels.ConsultantReimbursedBenefits
{
    public class GetApprovedBenefitsWhereConsultant
    {
        public int MovementId { get; set; }
        public int MovementTypeId { get; set; }
        public string MovementTypeName { get; set; }
        public decimal AmountReimbursed { get; set; }
    }
}
