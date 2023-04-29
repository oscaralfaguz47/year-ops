using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class CalculatorAccountingAccountToIgnoreRepository : Repository<CalculatorAccountingAccountToIgnore>, ICalculatorAccountingAccountToIgnoreRepository
    {
        private ApplicationDbContext _db;
        public CalculatorAccountingAccountToIgnoreRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
   

        public void Update(CalculatorAccountingAccountToIgnore obj)
        {
            _db.CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE.Update(obj);
        }

    }
}
