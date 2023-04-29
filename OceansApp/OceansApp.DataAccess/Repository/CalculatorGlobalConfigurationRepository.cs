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

        public CalculatorGlobalConfiguration GetGlobalConfiguration()
        {
            var globalConfig = _db.CALCULATOR_GLOBAL_CONFIGURATIONS.FirstOrDefault(x => x.Id == "Configuration1");
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
