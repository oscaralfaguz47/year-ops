
namespace OceansApp.Models.ViewModels.DocumentsCC
{
    public class DocumentCCGetExpiredDocsVM
    {
        public int DocumentCCId { get; set; }
        public DateTime DocumentDate { get; set; }
        public Decimal DocumentAmount { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int NumDaysExpired { get; set; }
        public Decimal BalanceAmount { get; set; }
        public string DocumentNumber { get; set; }
        public string ClientName { get; set; }
        public int NumNotificationsSent { get; set; }
        public string SuccessManagerEmail { get; set; }
    }
}
