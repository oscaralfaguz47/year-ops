using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where  T : class
    {
        Task<T> GetFirstOrDefaultAsync(
    Expression<Func<T, bool>> filter,
    Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
        Task<IEnumerable<T>> GetAllAsync(
    Expression<Func<T, bool>>? filter = null,
    string? includeProperties = null,
    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

        Task AddAsync(T entity);

        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entity);
    }
}
