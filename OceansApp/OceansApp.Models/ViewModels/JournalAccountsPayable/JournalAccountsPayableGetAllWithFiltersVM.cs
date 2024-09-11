
namespace OceansApp.Models.ViewModels.JournalAccountsPayable
{
    public class JournalAccountsPayableGetAllWithFiltersVM
    {
        public int JournalId { get; set; }
        public string SeatNumber { get; set; }
        public DateTime AccountingDate { get; set; }
        public DateTime StartDatePeriod { get; set; }
        public DateTime EndDatePeriod { get; set; }
        public string CompanyName { get; set; }
        public string TransactionStatusName { get; set; }
    }
}
