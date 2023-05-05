using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface ICountryRepository : IRepository<Country> 
    {
        bool UpdateIfExistAddIfNot(Country obj);
        void Update(Country obj);


    }
}
