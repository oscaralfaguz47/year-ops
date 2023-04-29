
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ICalculatorGlobalConfigurationRepository : IRepository<CalculatorGlobalConfiguration> 
    {
        CalculatorGlobalConfiguration GetGlobalConfiguration();
        void Update(CalculatorGlobalConfiguration obj);

    }
}
