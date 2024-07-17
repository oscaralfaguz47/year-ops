using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using System.Linq.Expressions;

namespace OceansApp.DataAccess.Repository
{
    public class TransactionStatusRepository : Repository<TransactionStatus>, ITransactionStatusRepository
    {
        private ApplicationDbContext _db;
        public TransactionStatusRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<List<TransactionStatus>> GetAllAsync(Expression<Func<TransactionStatus, bool>>? predicate = null,
             Func<IQueryable<TransactionStatus>, IOrderedQueryable<TransactionStatus>>? orderBy = null)
        {
            IQueryable<TransactionStatus> query = _db.TRANSACTION_STATUSES;
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            return await query.ToListAsync();
        }

    }
}
