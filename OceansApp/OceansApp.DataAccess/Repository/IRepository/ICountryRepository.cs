using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Countries;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ICountryRepository : IRepository<Country> 
    {
        IEnumerable<CountriesSelectVM> GetCountriesWhereConsultantsAre();
        bool UpdateIfExistAddIfNot(Country obj);
        void Update(Country obj);

    }
}
