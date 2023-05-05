using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository
{
    public class ProviderRepository : Repository<Provider>, IProviderRepository
    {
        private ApplicationDbContext _db;
        public ProviderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(Provider obj)
        {
            _db.PROVIDER.Update(obj);
        }

        public bool UpdateIfExistAddIfNot(Provider obj)
        {
            var existingProvider = GetFirstOrDefault(u => u.IdProvider == obj.IdProvider);
            if (existingProvider == null)
            {
                _db.PROVIDER.Add(obj);
                _db.SaveChanges();
                return true;
            }
            else
            {
                if (existingProvider.DateLastUpdate != obj.DateLastUpdate)
                {
                    existingProvider.Name = obj.Name;
                    existingProvider.Alias = obj.Alias;
                    existingProvider.Occupation = obj.Occupation;
                    existingProvider.Address = obj.Address;
                    existingProvider.Email = obj.Email;
                    existingProvider.AdmissionDate = obj.AdmissionDate;
                    existingProvider.Phone1 = obj.Phone1;
                    existingProvider.Phone2 = obj.Phone2;
                    existingProvider.Country = obj.Country;
                    existingProvider.IdProviderCategory = obj.IdProviderCategory;
                    existingProvider.Notes = obj.Notes;
                    existingProvider.IsActive = obj.IsActive;
                    existingProvider.DateLastUpdate = obj.DateLastUpdate;
                    existingProvider.CreationDate = obj.CreationDate;
                    return true;
                }
                return false;
            }
        }

    }
}
