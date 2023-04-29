using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ICostCenterRepository : IRepository<CostCenter> 
    {
        IEnumerable<CostCenter> GetCostCenterOfExpenses();
        bool UpdateIfExistAddIfNot(CostCenter obj);
        void Update(CostCenter obj);


    }
}
