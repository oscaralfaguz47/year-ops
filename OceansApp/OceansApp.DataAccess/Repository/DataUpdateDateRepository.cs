using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class DataUpdateRepository : Repository<DataUpdateDate>, IDataUpdateDateRepository
    {
        private ApplicationDbContext _db;
        public DataUpdateRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(DataUpdateDate obj)
        {
            _db.DATA_UPDATE_DATES.Update(obj);
        }

        public DataUpdateDate GetLastDate()
        {
            var latestDate = _db.DATA_UPDATE_DATES.OrderByDescending(x => x.Date).FirstOrDefault();
            return latestDate;
        }

    }
}
