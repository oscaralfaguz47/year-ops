using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class PaymentMethodRepository : Repository<PaymentMethod>, IPaymentMethodRepository
    {
        private ApplicationDbContext _db;
        public PaymentMethodRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(PaymentMethod obj)
        {
            _db.PAYMENT_METHODS.Update(obj);
        }

    }
}
