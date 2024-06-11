
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ICostCenterAccountingAccountRepository : IRepository<CostCenterAccountingAccount> 
    {
        Task<bool> AddCostCenterAccountingAccount(CostCenterAccountingAccount obj);
    }
}
