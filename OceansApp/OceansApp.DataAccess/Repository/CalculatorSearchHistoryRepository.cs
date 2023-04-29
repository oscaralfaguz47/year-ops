using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class CalculatorSearchHistoryRepository : Repository<CalculatorSearchHistory>, ICalculatorSearchHistoryRepository
    {
        private ApplicationDbContext _db;
        public CalculatorSearchHistoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

       
        public void Update(CalculatorSearchHistory obj)
        {
            _db.CALCULATOR_SEARCH_HISTORY.Update(obj);
        }
    }
}
