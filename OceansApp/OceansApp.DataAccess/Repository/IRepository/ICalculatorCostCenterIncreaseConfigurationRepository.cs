
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ICalculatorCostCenterIncreaseConfigurationRepository : IRepository<CalculatorCostCenterIncreaseConfiguration> 
    {
        void Update(CalculatorCostCenterIncreaseConfiguration obj);

    }
}
