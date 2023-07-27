using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Countries;

namespace OceansApp.DataAccess.Repository
{
    public class CountryRepository : Repository<Country>, ICountryRepository
    {
        private ApplicationDbContext _db;
        public CountryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        IEnumerable<CountriesSelectVM> ICountryRepository.GetCountriesWhereConsultantsAre()
        {
            IEnumerable<CountriesSelectVM> countriesList = _db.COUNTRY
                .FromSqlRaw(@"
            SELECT C.IdCountry, C.Name
            FROM COUNTRY C
            JOIN PROVIDER P ON C.IdCountry = P.IdCountry
            JOIN PROVIDER_CATEGORY PC ON P.Id = PC.Id
            WHERE PC.ProviderCategoryCode NOT IN ('PR', 'PROV', 'OCEANS', 'BONOS S')
            GROUP BY C.IdCountry, C.Name
        ")
                .Select(c => new CountriesSelectVM
                {
                    IdCountry = c.IdCountry,
                    Name = c.Name
                })
                .ToList();
            return countriesList;
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
