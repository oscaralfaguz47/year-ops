using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ProviderEventRepository : Repository<ProviderEvent>, IProviderEventRepository
    {
        private ApplicationDbContext _db;
        public ProviderEventRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(ProviderEvent obj)
        {
            _db.PROVIDER_EVENTS.Update(obj);
        }

    }
}
