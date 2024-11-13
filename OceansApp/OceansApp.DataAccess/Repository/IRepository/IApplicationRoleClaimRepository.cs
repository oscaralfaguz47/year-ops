
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IApplicationRoleClaimRepository : IRepository<ApplicationRoleClaim> 
    {
        Task<bool> ValidateRoleClaimAsync(string userId, string claimType, string claimValue);
    }
}
