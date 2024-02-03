using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
namespace OceansApp.DataAccess.Repository
{
    public class ProjectConsultantAssignedRepository : Repository<ProjectConsultantAssigned>, IProjectConsultantAssignedRepository
    {
        private ApplicationDbContext _db;
        public ProjectConsultantAssignedRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ProjectConsultantAssigned obj)
        {
            _db.PROJECTS_CONSULTANTS_ASSIGNED.Update(obj);
        }

    }
}
