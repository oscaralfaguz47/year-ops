using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;


namespace OceansApp.DataAccess.Repository
{
    public class ConsultantPaymentRepository : Repository<ConsultantPayment>, IConsultantPaymentRepository
    {
        private ApplicationDbContext _db;
        public ConsultantPaymentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


    }
}
