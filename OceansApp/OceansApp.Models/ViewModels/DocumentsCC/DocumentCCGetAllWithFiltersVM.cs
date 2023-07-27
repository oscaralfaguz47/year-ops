

namespace OceansApp.Models.ViewModels.DocumentsCC
{
    public class DocumentCCGetAllWithFiltersVM
    {
        public int DocumentCCId { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentType { get; set; }
        public string ApplicationDescription { get; set; }
        public DateTime DocumentDate { get; set; }
        public Decimal DocumentAmount { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int NumDaysToExpire { get; set; }
        public Decimal BalanceAmount { get; set; }
        public string Canceled { get; set; }
        public string ClientName { get; set; }
        public string CompanyId { get; set; }
        public string ClientCategory { get; set; }
    }
}
