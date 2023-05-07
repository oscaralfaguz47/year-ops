

namespace OceansApp.Models.ViewModels
{
    public class AccountingAccountWithBalanceVM
    {
        public string IdAccountingAccount { get; set; }
        public string AccountingAccountName { get; set; }
        public string IdCostCenter { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
