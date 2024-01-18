using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using System.Linq.Expressions;

namespace OceansApp.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;
        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(ApplicationUser obj)
        {
            _db.AspNetUsers.Update(obj);
        }
        public async Task<bool> AnyAsync(Expression<Func<ApplicationUser, bool>> predicate)
        {
            return await _db.AspNetUsers.AnyAsync(predicate);
        }

    }
}
