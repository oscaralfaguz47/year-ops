
using OceansApp.Models.Models;
using System.Linq.Expressions;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser> 
    {
        Task<bool> AnyAsync(Expression<Func<ApplicationUser, bool>> predicate);
        void Update(ApplicationUser obj);
    }
}
