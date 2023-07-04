using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantQualityLevelRepository : Repository<ConsultantQualityLevel>, IConsultantQualityLevelRepository
    {
        private ApplicationDbContext _db;
        public ConsultantQualityLevelRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
     
        public void Update(ConsultantQualityLevel obj)
        {
            _db.CONSULTANT_QUALITY_LEVELS.Update(obj);
        }

    }
}
