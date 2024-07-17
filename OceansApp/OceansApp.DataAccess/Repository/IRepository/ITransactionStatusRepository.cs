using OceansApp.Models.Models;
using System.Linq.Expressions;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ITransactionStatusRepository : IRepository<TransactionStatus> 
    {
        Task<List<TransactionStatus>> GetAllAsync(Expression<Func<TransactionStatus, bool>>? predicate = null,
             Func<IQueryable<TransactionStatus>, IOrderedQueryable<TransactionStatus>>? orderBy = null);
    }
}
