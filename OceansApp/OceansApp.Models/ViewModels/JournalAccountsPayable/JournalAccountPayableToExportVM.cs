
namespace OceansApp.Models.ViewModels.JournalAccountsPayable
{
    public class JournalAccountPayableToExportVM
    {
        public string Entry { get; set; }
        public string AccountingPackage { get; set; }
        public string EntryType { get; set; }
        public DateTime AccountingDate { get; set; }
        public string Accounting { get; set; } = "F";
        public List<JournalAccountPayableEntriesToExportVM> entriesList { get; set; }
    }
}
