using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ApplicationSystemClaimRepository : Repository<ApplicationSystemClaim>, IApplicationSystemClaimRepository
    {
        private ApplicationDbContext _db;
        public ApplicationSystemClaimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(ApplicationSystemClaim obj)
        {
            _db.APPLICATION_SYSTEM_CLAIMS.Update(obj);
        }

    }
}
