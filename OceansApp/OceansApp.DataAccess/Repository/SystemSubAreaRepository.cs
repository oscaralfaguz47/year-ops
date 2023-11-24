using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class SystemSubAreaRepository : Repository<SystemSubArea>, ISystemSubAreaRepository
    {
        private ApplicationDbContext _db;
        public SystemSubAreaRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(SystemSubArea obj)
        {
            _db.SYSTEM_SUB_AREAS.Update(obj);
        }

    }
}
