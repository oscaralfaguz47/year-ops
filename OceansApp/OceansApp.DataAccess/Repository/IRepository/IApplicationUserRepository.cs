
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Account;
using OceansApp.Models.ViewModels.ApplicationUser;
using OceansApp.Models.ViewModels.Dashboard;
using System.Linq.Expressions;
using System.Security.Claims;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser> 
    {
        Task<bool> AnyAsync(Expression<Func<ApplicationUser, bool>> predicate);
        Task<List<GetUserIdVM>> GetUsersWhereRoleId(string roleId);
        Task<UserAndConsultantVM> GetUserAndConsultantAsync(string userId);
        Task<List<WidgetVM>> GetWidgetsForUserAsync(UserAndConsultantVM userAndConsultant, ClaimsPrincipal userClaims);
    }
}
