
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.AdminCenter.UserRolesPermissions;
using System.Linq.Expressions;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IApplicationSystemClaimRepository : IRepository<ApplicationSystemClaim> 
    {
        void Update(ApplicationSystemClaim obj);
        Task<ApplicationSystemClaim> GetFirstOrDefaultAsync(Expression<Func<ApplicationSystemClaim, bool>> filter);
        Task<List<GetClaimsVM>> GetClaimsListWhereRole(string roleId);
        IEnumerable<GetClaimsVM> GetAllPermissionsCustomData();
    }
}
