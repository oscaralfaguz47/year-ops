namespace OceansApp.Models.ViewModels.ConsultantReimbursedBenefits
{
    public class ConsultantReimbursedBenefitsFiltersGetAllVM
    {
        public string? SearchText { get; set; }
        public int? BenefitId { get; set; }
        public int? BenefitCategoryId { get; set; }
        public int? TransactionStatusId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
