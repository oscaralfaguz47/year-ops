using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;


namespace OceansApp.DataAccess.Repository
{
    public class ApplicationUserActiveHistoryRepository : Repository<ApplicationUserActiveHistory>, IApplicationUserActiveHistoryRepository
    {
        private ApplicationDbContext _db;
        public ApplicationUserActiveHistoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        

    }
}
