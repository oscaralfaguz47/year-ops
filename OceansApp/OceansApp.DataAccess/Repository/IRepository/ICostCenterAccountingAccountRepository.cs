
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ICostCenterAccountingAccountRepository : IRepository<CostCenterAccountingAccount> 
    {
        public bool AddCostCenterAccountingAccount(CostCenterAccountingAccount obj);
    }
}
