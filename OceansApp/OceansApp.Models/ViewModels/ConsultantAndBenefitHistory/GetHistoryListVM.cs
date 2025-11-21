
namespace OceansApp.Models.ViewModels.ConsultantAndBenefitHistory
{
    public class GetHistoryListVM
    {
        public string? BenefitCategory { get; set; }
        public decimal OldValue { get; set; }
        public decimal NewValue { get; set; }
        public string? ReimbursementDetail { get; set; }
        public string? Notes { get; set; }
        public string UserCreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
