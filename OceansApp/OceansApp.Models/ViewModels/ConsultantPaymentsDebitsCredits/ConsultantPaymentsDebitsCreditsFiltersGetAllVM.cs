namespace OceansApp.Models.ViewModels.ConsultantPaymentsDebitsCredits
{
    public class ConsultantPaymentsDebitsCreditsFiltersGetAllVM
    {
        public string? SearchText { get; set; }
        public int? TransactionStatusId { get; set; }
        public int? TransactionTypeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
