
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ICalculatorGlobalConfigurationRepository : IRepository<CalculatorGlobalConfiguration> 
    {
        Task<CalculatorGlobalConfiguration> GetGlobalConfiguration();
        void Update(CalculatorGlobalConfiguration obj);

    }
}
