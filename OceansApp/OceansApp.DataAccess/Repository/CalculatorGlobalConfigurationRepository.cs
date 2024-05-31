using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class CalculatorGlobalConfigurationRepository : Repository<CalculatorGlobalConfiguration>, ICalculatorGlobalConfigurationRepository
    {
        private ApplicationDbContext _db;
        public CalculatorGlobalConfigurationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<CalculatorGlobalConfiguration> GetGlobalConfiguration()
        {
            var globalConfig = await _db.CALCULATOR_GLOBAL_CONFIGURATIONS.FirstOrDefaultAsync(x => x.Id == "Configuration1");
            if (globalConfig != null)
            {
                return globalConfig;
            }
            else
            {
                return new CalculatorGlobalConfiguration();
            }
        }
        public void Update(CalculatorGlobalConfiguration obj)
        {
            _db.CALCULATOR_GLOBAL_CONFIGURATIONS.Update(obj);
        }
    }
}
