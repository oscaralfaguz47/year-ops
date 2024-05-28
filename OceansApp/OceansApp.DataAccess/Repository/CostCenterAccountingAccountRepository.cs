using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class CostCenterAccountingAccountRepository : Repository<CostCenterAccountingAccount>, ICostCenterAccountingAccountRepository
    {
        private ApplicationDbContext _db;
        public CostCenterAccountingAccountRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public bool AddCostCenterAccountingAccount(CostCenterAccountingAccount obj)
        {
            var existingCostCenterAccount = GetFirstOrDefault(u => u.CostCenterId == obj.CostCenterId && u.AccountingAccountId == obj.AccountingAccountId && u.CompanyId == obj.CompanyId);
            if (existingCostCenterAccount == null)
            {
                _db.COSTS_CENTERS_ACCOUNTING_ACCOUNTS.Add(obj);
                _db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
