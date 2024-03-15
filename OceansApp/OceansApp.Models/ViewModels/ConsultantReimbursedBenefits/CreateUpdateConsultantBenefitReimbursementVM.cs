
namespace OceansApp.Models.ViewModels.ConsultantReimbursedBenefits
{
    public class CreateUpdateConsultantBenefitReimbursementVM
    {
        public int? ReimbursedBenefitId { get; set; }
        public int? BenefitId { get; set; }
        public string? Detail { get; set; }
        public int? ConsultantId { get; set; }
        public decimal? AmountReimbursed { get; set; }
        public DateTime? DateToBeReimbursed { get; set; }
        public int? BenefitCategoryId { get; set; }
    }
}
