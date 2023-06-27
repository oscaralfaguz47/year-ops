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
            // Primera consulta: obtener las categorías y el número de proveedores por categoría
            var categoryQuery = @"
        SELECT PC.IdProviderCategory, PC.Description, COUNT(PC.Description) AS NumProviders
        FROM PROVIDER P
        JOIN PROVIDER_CATEGORY PC ON P.IdProviderCategory = PC.IdProviderCategory
        WHERE P.IsActive = @ProviderIsActive AND PC.IdProviderCategory NOT IN('PR', 'OCEANS')
        GROUP BY PC.IdProviderCategory, PC.Description
        ORDER BY PC.Description";

            // Segunda consulta: obtener los proveedores por categoría
            var providerQuery = @"
        SELECT P.IdProvider, P.Name, P.Occupation, PC.IdProviderCategory
        FROM PROVIDER P
        JOIN PROVIDER_CATEGORY PC ON P.IdProviderCategory = PC.IdProviderCategory
        WHERE P.IsActive = @ProviderIsActive AND PC.IdProviderCategory NOT IN('PR', 'OCEANS')
        ORDER BY PC.Description";

            var results = new List<ProviderGroupByCategoryVM>();

            using var connection = _db.Database.GetDbConnection();
            await connection.OpenAsync();

            // Ejecuta la primera consulta para obtener las categorías y el número de proveedores
            using var categoryCommand = connection.CreateCommand();
            categoryCommand.CommandText = categoryQuery;
            categoryCommand.Parameters.Add(new SqlParameter("@ProviderIsActive", providerIsActive));

            using var categoryReader = await categoryCommand.ExecuteReaderAsync();
            while (await categoryReader.ReadAsync())
            {
                results.Add(new ProviderGroupByCategoryVM
                {
                    IdCategory = categoryReader.GetString(0),
                    CategoryDescription = categoryReader.GetString(1),
                    NumProviders = categoryReader.GetInt32(2),
                    Providers = new List<ProviderGetAllVM>()
                });
            }
            await connection.CloseAsync();

            // Ejecuta la segunda consulta para obtener los proveedores por categoría
            await connection.OpenAsync();
            using var providerCommand = connection.CreateCommand();
            providerCommand.CommandText = providerQuery;
            providerCommand.Parameters.Add(new SqlParameter("@ProviderIsActive", providerIsActive));

            using var providerReader = await providerCommand.ExecuteReaderAsync();
            while (await providerReader.ReadAsync())
            {
                var provider = new ProviderGetAllVM
                {
                    IdProvider = providerReader.GetString(0),
                    Name = providerReader.GetString(1),
                    Occupation = providerReader.GetString(2)
                };

                var categoryId = providerReader.GetString(3);
                var category = results.FirstOrDefault(x => x.IdCategory == categoryId);

                if (category != null)
                {
                    category.Providers.Add(provider);
                }
            }
            await connection.CloseAsync();

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
