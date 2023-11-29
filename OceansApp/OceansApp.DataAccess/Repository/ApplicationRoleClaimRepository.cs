using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ApplicationRoleClaimRepository : Repository<ApplicationRoleClaim>, IApplicationRoleClaimRepository
    {
        private ApplicationDbContext _db;
        public ApplicationRoleClaimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

    }
}
