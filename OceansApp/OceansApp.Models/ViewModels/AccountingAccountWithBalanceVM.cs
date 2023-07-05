

namespace OceansApp.Models.ViewModels
{
    public class AccountingAccountWithBalanceVM
    {
        public int AccountingAccountId { get; set; }
        public string AccountingAccountCode { get; set; }
        public string AccountingAccountName { get; set; }
        public int CostCenterId { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
