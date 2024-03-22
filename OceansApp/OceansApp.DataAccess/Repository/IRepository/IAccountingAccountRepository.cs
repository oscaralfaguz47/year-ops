using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.AccountingAccounts;

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
        Task<List<GetAccountingAccountsForListVM>> GetAccountingAccountsWhereCostCenterIdAsync(int costCenterId);
    }
}
