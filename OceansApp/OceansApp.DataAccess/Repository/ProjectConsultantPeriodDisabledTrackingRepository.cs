using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;


namespace OceansApp.DataAccess.Repository
{
    public class ProjectConsultantPeriodDisabledTrackingRepository : Repository<ProjectConsultantPeriodDisabledTracking>, IProjectConsultantPeriodDisabledTrackingRepository
    {
        private ApplicationDbContext _db;
        public ProjectConsultantPeriodDisabledTrackingRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
