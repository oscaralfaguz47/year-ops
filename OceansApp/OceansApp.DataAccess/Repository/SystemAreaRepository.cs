using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class SystemAreaRepository : Repository<SystemArea>, ISystemAreaRepository
    {
        private ApplicationDbContext _db;
        public SystemAreaRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(SystemArea obj)
        {
            _db.SYSTEM_AREAS.Update(obj);
        }

    }
}
