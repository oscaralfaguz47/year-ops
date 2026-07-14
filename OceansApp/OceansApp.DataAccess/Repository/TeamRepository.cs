using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class TeamRepository : Repository<Team>, ITeamRepository
    {
        private ApplicationDbContext _db;
        public TeamRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Team obj)
        {
            _db.TEAMS.Update(obj);
        }
    }
}
