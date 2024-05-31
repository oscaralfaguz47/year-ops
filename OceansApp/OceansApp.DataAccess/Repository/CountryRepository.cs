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
        public async Task<IEnumerable<CountriesSelectVM>> GetCountriesWhereConsultantsAreAsync()
        {
            IEnumerable<CountriesSelectVM> countriesList = await _db.COUNTRY
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
                .ToListAsync();

            return countriesList;
        }


        public async Task<bool> UpdateIfExistAddIfNot(Country obj)
        {
            var existingCountry = await GetFirstOrDefaultAsync(u => u.IdCountry == obj.IdCountry);
            if (existingCountry == null)
            {
               await _db.COUNTRY.AddAsync(obj);
               await _db.SaveChangesAsync();
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
