namespace OceansApp.Models.ViewModels.TimeOff
{
    public class ApproveRejectTimeOffVM
    {
        public int? TimeOffRequestId { get; set; }
        public string? TransactionStatus { get; set; }
        public string? RejectionComment { get; set; }
    }
}
