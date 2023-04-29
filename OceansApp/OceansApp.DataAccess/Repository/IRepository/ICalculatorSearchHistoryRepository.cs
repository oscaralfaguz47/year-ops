
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ICalculatorSearchHistoryRepository : IRepository<CalculatorSearchHistory> 
    {
        void Update(CalculatorSearchHistory obj);

    }
}
