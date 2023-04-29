using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ILedgerMovementRepository : IRepository<LedgerMovement> 
    {
        Decimal GetDebitAndCreditAmountOfAnAccountingAccount(String accountingAccount,
            String? costCenter, DateTime StartDate, DateTime endDate, String balance);
        void Update(LedgerMovement obj);
        bool AddIfNotExist(LedgerMovement obj);
    }
}
