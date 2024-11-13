using OceansApp.Models.Models;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;

namespace OceansApp.DataAccess.Repository
{
    public class AccountPayableRepository : Repository<AccountPayable>, IAccountPayableRepository
    {
        private ApplicationDbContext _db;
        public AccountPayableRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
