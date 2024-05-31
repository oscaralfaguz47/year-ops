using System.Linq.Expressions;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where  T : class
    {
        // T = CuentaContable
        Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);

        Task AddAsync(T entity);

        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entity);
    }
}
