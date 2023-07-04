using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantRoleRepository : Repository<ConsultantRole>, IConsultantRoleRepository
    {
        private ApplicationDbContext _db;
        public ConsultantRoleRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
     

        public void Update(ConsultantRole obj)
        {
            _db.CONSULTANT_ROLES.Update(obj);
        }

    }
}
