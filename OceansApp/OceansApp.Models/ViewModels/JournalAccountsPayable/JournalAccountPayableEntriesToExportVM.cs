
namespace OceansApp.Models.ViewModels.JournalAccountsPayable
{
    public class JournalAccountPayableEntriesToExportVM
    {
        public string Nit { get; set; } = "ND";
        public string CostCenter { get; set; }
        public string AccountingAccount{ get; set; }
        public string Source { get; set; }
        public string Reference { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }
}
