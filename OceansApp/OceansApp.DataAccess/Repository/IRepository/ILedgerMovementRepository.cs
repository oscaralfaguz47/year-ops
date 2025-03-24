using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;
using OceansApp.Models.ViewModels.DataFromSoftland;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ILedgerMovementRepository : IRepository<LedgerMovement> 
    {
        Task<IEnumerable<AccountingAccountWithBalanceVM>> GetAccountingAccountsWithBalance(string accountingAccountIdBegin,
            DateTime fechaInicial, DateTime fechaFinal, int ignoreAccountingAccounts, string balance);

        Task<IEnumerable<AccountingAccountWithBalanceVM>> GetAccountingAccountsReturnsAndDiscountsWithBalance(
            DateTime fechaInicial, DateTime fechaFinal, int ignoreAccountingAccounts);
        void Update(LedgerMovement obj);
        Task<int> AddIfNotExistBulkAsync(IEnumerable<CreateLedgerMovementVM> movements);
    }
}
