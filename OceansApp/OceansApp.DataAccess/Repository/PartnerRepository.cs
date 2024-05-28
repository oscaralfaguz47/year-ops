using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using System.Linq.Expressions;

namespace OceansApp.DataAccess.Repository
{
    public class PartnerRepository : Repository<Partner>, IPartnerRepository
    {
        private ApplicationDbContext _db;
        public PartnerRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<List<Partner>> GetAllAsync(Expression<Func<Partner, bool>>? predicate = null)
        {
            IQueryable<Partner> query = _db.PARTNERS;
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.ToListAsync();
        }

    }
}
