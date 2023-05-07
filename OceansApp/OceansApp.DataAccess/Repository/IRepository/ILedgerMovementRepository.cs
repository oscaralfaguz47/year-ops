using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ILedgerMovementRepository : IRepository<LedgerMovement> 
    {
        Task<IEnumerable<AccountingAccountWithBalanceVM>> GetAccountingAccountsWithBalance(string accountingAccountIdBegin,
            DateTime fechaInicial, DateTime fechaFinal, int ignoreAccountingAccounts, string balance);

        Task<IEnumerable<AccountingAccountWithBalanceVM>> GetAccountingAccountsReturnsAndDiscountsWithBalance(
            DateTime fechaInicial, DateTime fechaFinal, int ignoreAccountingAccounts);
        void Update(LedgerMovement obj);
        bool AddIfNotExist(LedgerMovement obj);
    }
}
