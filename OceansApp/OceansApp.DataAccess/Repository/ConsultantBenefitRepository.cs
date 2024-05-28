using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantBenefitRepository : Repository<ConsultantBenefit>, IConsultantBenefitRepository
    {
        private ApplicationDbContext _db;
        public ConsultantBenefitRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
