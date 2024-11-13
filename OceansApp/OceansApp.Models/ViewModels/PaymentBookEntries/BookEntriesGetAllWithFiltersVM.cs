
namespace OceansApp.Models.ViewModels.PaymentBookEntries
{
    public class BookEntriesGetAllWithFiltersVM
    {
        public int ParentId { get; set; }
        public DateTime CreationDate { get; set; }
        public string CompanyName { get; set; }
        public string TransactionStatusName { get; set; }
        public int NumValidChildren { get; set; }
        public int NumVoidedChildren { get; set; }
    }
}
