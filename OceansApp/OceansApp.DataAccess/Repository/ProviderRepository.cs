using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;

namespace OceansApp.DataAccess.Repository
{
    public class ProviderRepository : Repository<Provider>, IProviderRepository
    {
        private ApplicationDbContext _db;
        public ProviderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<List<ProviderGroupByCategoryVM>> GetProvidersGroupByCategoryAsync(string providerIsActive)
        {
            var query = @"
                SELECT PC.IdProviderCategory, PC.Description, COUNT(PC.Description) AS NumProviders
                FROM PROVIDER P
                JOIN PROVIDER_CATEGORY PC ON P.IdProviderCategory = PC.IdProviderCategory
                WHERE P.IsActive = @ProviderIsActive AND PC.IdProviderCategory NOT IN('PR', 'OCEANS')
                GROUP BY PC.IdProviderCategory, PC.Description ORDER BY PC.Description";

            var results = new List<ProviderGroupByCategoryVM>();

            using var connection = _db.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = query;
            command.Parameters.Add(new SqlParameter("@ProviderIsActive", providerIsActive));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new ProviderGroupByCategoryVM
                {
                    IdCategory = reader.GetString(0), // Asume que IdProviderCategory es de tipo string.
                    CategoryDescription = reader.GetString(1),
                    NumProviders = reader.GetInt32(2)
                });
            }

            return results;
        }
        public async Task<List<ProviderGroupByCategoryVM>> GetWantedProvidersAsync(string providerIsActive)
        {
            var query = @"
                SELECT 
                IdProvider,
                Name, 
                Occupation, 
                IdProviderCategory
                FROM PROVIDER 
                WHERE IsActive = @ProviderIsActive 
                AND IdProviderCategory NOT IN('PR', 'OCEANS')";

            var results = new List<ProviderGroupByCategoryVM>();

            using var connection = _db.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = query;
            command.Parameters.Add(new SqlParameter("@ProviderIsActive", providerIsActive));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new ProviderGroupByCategoryVM
                {
                    IdCategory = reader.GetString(0), // Asume que IdProviderCategory es de tipo string.
                    CategoryDescription = reader.GetString(1),
                    NumProviders = reader.GetInt32(2)
                });
            }

            return results;
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
