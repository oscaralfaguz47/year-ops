using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;
using OceansApp.Models.ViewModels.Providers;

using Dapper;

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
        SELECT PC.ProviderCategoryCode, PC.Description, COUNT(PC.Description) AS NumProviders
        FROM PROVIDER P
        JOIN PROVIDER_CATEGORY PC ON P.Id = PC.Id
        WHERE P.IsActive = @ProviderIsActive AND PC.ProviderCategoryCode NOT IN('PR', 'OCEANS', 'PROV')
        GROUP BY PC.ProviderCategoryCode, PC.Description
        ORDER BY PC.Description";

            // Segunda consulta: obtener los proveedores por categoría
            var providerQuery = @"
        SELECT P.ProviderCode, P.Name, P.Occupation, PC.ProviderCategoryCode, P.CompanyId
        FROM PROVIDER P
        JOIN PROVIDER_CATEGORY PC ON P.Id = PC.Id
        WHERE P.IsActive = @ProviderIsActive AND PC.ProviderCategoryCode NOT IN('PR', 'OCEANS')
        ORDER BY P.Name";

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
                    Occupation = providerReader.GetString(2),
                    Company = providerReader.GetString(4)
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

        public async Task<List<ProviderGetAllWithFiltersVM>> GetAllProviderWithFiltersAsync(ProviderFiltersGetAllVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var query = @"
    DECLARE @pageNumberParam INT = @PageNumber;
    DECLARE @pageSizeParam INT = @PageSize;
    DECLARE @numRowsToSkip INT = ((@pageNumberParam - 1) * @pageSizeParam);
    DECLARE @isActiveParam NVARCHAR(1) = @IsActive;
    DECLARE @nameOrAliasParam NVARCHAR(150) = @NameOrAlias;
    DECLARE @countryIdParam NVARCHAR(4) = @CountryId;
    DECLARE @clientIdParam INT = @ClientId;
    DECLARE @companyIdParam NVARCHAR(8) = @CompanyId;

    SELECT P.Name,
        P.Alias,
        P.Occupation,
        P.Address,
        P.Email,
        P.AdmissionDate,
        P.Phone1,
        P.Phone2,
        P.IdCountry,
        P.Notes,
        P.IsActive,
        CP.Description AS CategoryDescription ,
        P.CompanyId,
        C.Name AS ClientName
    FROM PROVIDER P
    JOIN PROVIDER_CATEGORY CP ON P.Id = CP.Id
    JOIN CLIENT C ON P.ClientId = C.ClientId
    WHERE (@isActiveParam IS NULL OR P.IsActive = @isActiveParam)
    AND ((@nameOrAliasParam IS NULL OR LOWER(P.Name) LIKE '%' + LOWER(@nameOrAliasParam) + '%')
        OR (@nameOrAliasParam IS NULL OR LOWER(P.Alias) LIKE '%' + LOWER(@nameOrAliasParam) + '%'))
    AND (@countryIdParam IS NULL OR P.IdCountry = @countryIdParam)
    AND (@clientIdParam IS NULL OR P.ClientId = @clientIdParam)
    AND (@companyIdParam IS NULL OR P.CompanyId = @companyIdParam)
    ORDER BY P.Name
    OFFSET @numRowsToSkip ROWS
    FETCH NEXT @pageSizeParam ROWS ONLY";

            var parameters = new
            {
                PageNumber = filtersAndPagination.Pagination.PageNumber,
                PageSize = filtersAndPagination.Pagination.PageSize,
                IsActive = filtersAndPagination.IsActive,
                NameOrAlias = filtersAndPagination.NameOrAlias,
                CountryId = filtersAndPagination.CountryId,
                ClientId = filtersAndPagination.ClientId,
                CompanyId = filtersAndPagination.CompanyId
            };

            var results = await connection.QueryAsync<ProviderGetAllWithFiltersVM>(query, parameters);

            return results.ToList();
        }


        public void Update(Provider obj)
        {
            _db.PROVIDER.Update(obj);
        }

        public int? UpdateIfExistAddIfNot(Provider obj)
        {
            var existingProvider = GetFirstOrDefault(u => u.ProviderCode == obj.ProviderCode && u.CompanyId == obj.CompanyId);
            if (existingProvider == null)
            {
                _db.PROVIDER.Add(obj);
                _db.SaveChanges();
                return obj.ProviderId;
            }
            else
            {
                if (!existingProvider.DateLastUpdate.Equals(obj.DateLastUpdate))
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
                    existingProvider.ClientId = obj.ClientId;
                    return existingProvider.ProviderId;
                }
                return null;
            }
        }

    }
}
