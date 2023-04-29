
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ICalculatorAccountingAccountToIgnoreRepository : IRepository<CalculatorAccountingAccountToIgnore> 
    {
        void Update(CalculatorAccountingAccountToIgnore obj);


    }
}
