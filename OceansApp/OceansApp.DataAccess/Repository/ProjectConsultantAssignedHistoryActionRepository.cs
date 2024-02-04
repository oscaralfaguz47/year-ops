using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
namespace OceansApp.DataAccess.Repository
{
    public class ProjectConsultantAssignedHistoryActionRepository : Repository<ProjectConsultantAssignedHistoryAction>, IProjectConsultantAssignedHistoryActionRepository
    {
        private ApplicationDbContext _db;
        public ProjectConsultantAssignedHistoryActionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ProjectConsultantAssignedHistoryAction obj)
        {
            _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS.Update(obj);
        }

    }
}
