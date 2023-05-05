using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class CountryRepository : Repository<Country>, ICountryRepository
    {
        private ApplicationDbContext _db;
        public CountryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
     
        public bool UpdateIfExistAddIfNot(Country obj)
        {
            var existingCountry = GetFirstOrDefault(u => u.IdCountry == obj.IdCountry);
            if (existingCountry == null)
            {
                _db.COUNTRY.Add(obj);
                _db.SaveChanges();
                return true;
            }
            else
            {
                if (existingCountry.Name != obj.Name)
                {
                    existingCountry.Name = obj.Name;
                    return true;
                }
                return false;
            }
        }

        public void Update(Country obj)
        {
            _db.COUNTRY.Update(obj);
        }

    }
}
