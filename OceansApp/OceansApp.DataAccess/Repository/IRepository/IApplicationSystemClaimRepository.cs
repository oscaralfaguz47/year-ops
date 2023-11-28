
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.AdminCenter.UserRolesPermissions;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IApplicationSystemClaimRepository : IRepository<ApplicationSystemClaim> 
    {
        void Update(ApplicationSystemClaim obj);
        Task<List<GetClaimsVM>> GetClaimsListWhereRole(string roleId);
        IEnumerable<GetPermissionsListVM> GetAllPermissionsCustomData();
    }
}
