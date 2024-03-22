using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.CostsCenters;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ICostCenterRepository : IRepository<CostCenter> 
    {
        IEnumerable<CostCenter> GetCostCenterOfExpenses();
        bool UpdateIfExistAddIfNot(CostCenter obj);
        void Update(CostCenter obj);
        Task<List<GetCostsCentersForListVM>> GetCostsCentersWhereCompanyIdAsync(string companyId);


    }
}
