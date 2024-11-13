using OceansApp.Models.Models;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;

namespace OceansApp.DataAccess.Repository
{
    public class AccountPayableMovementRepository : Repository<AccountPayableMovement>, IAccountPayableMovementRepository
    {
        private ApplicationDbContext _db;
        public AccountPayableMovementRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


    }
}
