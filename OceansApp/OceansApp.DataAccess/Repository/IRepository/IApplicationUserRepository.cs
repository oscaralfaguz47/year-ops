
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Account;
using System.Linq.Expressions;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser> 
    {
        Task<bool> AnyAsync(Expression<Func<ApplicationUser, bool>> predicate);
        void Update(ApplicationUser obj);
        Task<List<GetUserIdVM>> GetUsersWhereRoleId(string roleId);
    }
}
