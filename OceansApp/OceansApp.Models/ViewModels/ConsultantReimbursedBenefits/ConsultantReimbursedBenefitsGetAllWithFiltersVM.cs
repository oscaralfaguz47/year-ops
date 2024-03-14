
namespace OceansApp.Models.ViewModels.ConsultantReimbursedBenefits
{
    public class ConsultantReimbursedBenefitsGetAllWithFiltersVM
    {
        public int ReimbursedBenefitId { get; set; }
        public string ConsultantName { get; set; }
        public string BenefitName { get; set; }
        public decimal AmountReimbursed { get; set; }
        public string? Detail { get; set; }
        public DateTime DateToBeReimbursed { get; set; }
        public bool BenefitPaid { get; set; }
        public string UserCreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string? UserLastUpdatedBy { get; set; }
        public DateTime? LastUpdateDate { get; set; }
    }
}
