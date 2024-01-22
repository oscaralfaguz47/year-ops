using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantPositionRepository : Repository<ConsultantPosition>, IConsultantPositionRepository
    {
        private ApplicationDbContext _db;
        public ConsultantPositionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
     

        public void Update(ConsultantPosition obj)
        {
            _db.CONSULTANT_POSITIONS.Update(obj);
        }

    }
}
