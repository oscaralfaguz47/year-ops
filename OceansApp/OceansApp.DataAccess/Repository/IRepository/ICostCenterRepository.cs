using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.CostsCenters;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ICostCenterRepository : IRepository<CostCenter> 
    {
        Task<IEnumerable<CostCenter>> GetCostCenterOfExpensesAsync();
        Task<bool> UpdateIfExistAddIfNot(CostCenter obj);
        void Update(CostCenter obj);
        Task<List<GetCostsCentersForListVM>> GetCostsCentersWhereCompanyIdAsync(string companyId);


    }
}
