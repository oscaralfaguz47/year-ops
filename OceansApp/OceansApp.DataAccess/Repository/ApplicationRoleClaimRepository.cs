using Microsoft.AspNetCore.Identity;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ApplicationRoleClaimRepository : Repository<ApplicationRoleClaim>, IApplicationRoleClaimRepository
    {
        private ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ApplicationRoleClaimRepository(ApplicationDbContext db, UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager) : base(db)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<bool> ValidateRoleClaimAsync(string userId, string claimType, string claimValue)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                var identityRole = await _roleManager.FindByNameAsync(role);
                if (identityRole != null)
                {
                    var claims = await _roleManager.GetClaimsAsync(identityRole);
                    // Validate if the claim matches
                    if (claims.Any(c => c.Type == claimType && c.Value == claimValue))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}
