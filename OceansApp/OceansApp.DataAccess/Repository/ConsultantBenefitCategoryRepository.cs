using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantBenefitCategoryRepository : Repository<ConsultantBenefitCategory>, IConsultantBenefitCategoryRepository
    {
        private ApplicationDbContext _db;
        public ConsultantBenefitCategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
