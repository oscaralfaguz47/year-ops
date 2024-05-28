using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ProjectUserSelectedRepository : Repository<ProjectUserSelected>, IProjectUserSelectedRepository
    {
        private ApplicationDbContext _db;
        public ProjectUserSelectedRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
