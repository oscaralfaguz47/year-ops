using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantRoleQualityLevelRepository : Repository<ConsultantRolesQualityLevels>, IConsultantRoleQualityLevelRepository
    {
        private ApplicationDbContext _db;
        public ConsultantRoleQualityLevelRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
     

        public void Update(ConsultantRolesQualityLevels obj)
        {
            _db.CONSULTANT_ROLES_QUALITY_LEVELS.Update(obj);
        }

    }
}
