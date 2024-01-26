using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ApplicationUserCategoryRepository : Repository<ApplicationUserCategory>, IApplicationUserCategoryRepository
    {
        private ApplicationDbContext _db;
        public ApplicationUserCategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(ApplicationUserCategory obj)
        {
            _db.UserCategories.Update(obj);
        }

    }
}
