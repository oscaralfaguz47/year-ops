namespace OceansApp.Models.ViewModels.TimeOff
{
    public class TimeOffFiltersVM
    {
        public string? SearchText { get; set; }
        public string? TimeOffType { get; set; }
        public int? TransactionStatusId { get; set; }
        public string? StatusName { get; set; }
        public int? ProjectId { get; set; }
        public int? ConsultantId { get; set; }
    }
}
