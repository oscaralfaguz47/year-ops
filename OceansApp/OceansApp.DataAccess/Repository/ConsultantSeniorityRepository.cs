using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantSeniorityRepository : Repository<ConsultantSeniority>, IConsultantSeniorityRepository
    {
        private ApplicationDbContext _db;
        public ConsultantSeniorityRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
     

        public void Update(ConsultantSeniority obj)
        {
            _db.CONSULTANT_SENIORITIS.Update(obj);
        }

    }
}
