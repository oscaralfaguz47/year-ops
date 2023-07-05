using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ProviderEventDateRepository : Repository<ProviderEventDate>, IProviderEventDateRepository
    {
        private ApplicationDbContext _db;
        public ProviderEventDateRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(ProviderEventDate obj)
        {
            _db.PROVIDER_EVENT_DATES.Update(obj);
        }

    }
}
