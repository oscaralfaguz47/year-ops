
namespace OceansApp.Models.ViewModels.PaymentBookEntries
{
    public class BookEntriesToExportVM
    {
        public string BankAccount { get; set; }
        public DateTime AccountingDate { get; set; }
        public string ReferenceNumber { get; set; }
        public string Notes { get; set; }
        public decimal PaymentAmount { get; set; }
        public string DocumentType { get; set; } = "O/D";
        public int DocumentSubType { get; set; }
        public string EntryType { get; set; }
        public string TaxCode { get; set; }

    }
}
