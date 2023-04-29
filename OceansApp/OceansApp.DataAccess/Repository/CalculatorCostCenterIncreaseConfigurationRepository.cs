using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class CalculatorCostCenterIncreaseConfigurationRepository : Repository<CalculatorCostCenterIncreaseConfiguration>, ICalculatorCostCenterIncreaseConfigurationRepository
    {
        private ApplicationDbContext _db;
        public CalculatorCostCenterIncreaseConfigurationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

       
        public void Update(CalculatorCostCenterIncreaseConfiguration obj)
        {
            _db.CALCULATOR_COST_CENTER_INCREASE_CONFIGURATIONS.Update(obj);
        }
    }
}
