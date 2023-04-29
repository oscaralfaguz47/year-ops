using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IAccountingAccountRepository : IRepository<AccountingAccount> 
    {
        IEnumerable<AccountingAccount> GetCostOfSalesAccountingAccounts();
        IEnumerable<AccountingAccount> GetExpensesAccountingAccounts();
        IEnumerable<AccountingAccount> GetReturnsAndDiscountsAccountingAccounts();
        bool UpdateIfExistAddIfNot(AccountingAccount obj);
        void Update(AccountingAccount obj);

        DateTime GetLatestUpdateDate();
    }
}
