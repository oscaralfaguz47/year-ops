using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class LedgerMovementRepository : Repository<LedgerMovement>, ILedgerMovementRepository
    {
        private ApplicationDbContext _db;
        public LedgerMovementRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public Decimal GetDebitAndCreditAmountOfAnAccountingAccount(String accountingAccount,
            String? costCenter, DateTime StartDate, DateTime endDate, String balance)
        {
            Decimal totalResult = 0;
            Decimal resultFromDb = _db.LEDGER_MOVEMENT.Where(x => (x.Date >= StartDate && x.Date <= endDate)
            && (costCenter == null || x.IdCostCenter == costCenter) && x.IdAccountingAccount == accountingAccount).AsEnumerable()
              .Sum(s => s.LocalDebit - s.LocalCredit);

            totalResult = resultFromDb;

            if (balance == "A" && resultFromDb > 0)
            {
                totalResult = resultFromDb * -1;
            }
            if (balance == "A" && resultFromDb < 0)
            {
                totalResult = Math.Abs(resultFromDb);
            }
            return totalResult;
        }
        public void Update(LedgerMovement obj)
        {
            _db.LEDGER_MOVEMENT.Update(obj);
        }
        public bool AddIfNotExist(LedgerMovement obj)
        {
            var existingLedgerMovement = GetFirstOrDefault(u => u.IdSeat == obj.IdSeat && u.IdCostCenter == obj.IdCostCenter &&
            u.IdAccountingAccount == obj.IdAccountingAccount && u.LocalDebit == obj.LocalDebit && 
            u.LocalCredit == obj.LocalCredit && u.Consecutive == obj.Consecutive);

            if (existingLedgerMovement == null)
            {
                _db.LEDGER_MOVEMENT.Add(obj);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
