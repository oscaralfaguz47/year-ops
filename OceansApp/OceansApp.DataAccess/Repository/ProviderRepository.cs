using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels;
using OceansApp.Models.ViewModels.Providers;

using Dapper;
using System.Text;
using System.Data;

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

        public async Task<(List<ProviderGetAllWithFiltersVM> providers, int totalCount)> GetAllProviderWithFiltersAsync(ProviderGetAllForListVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var queryBuilder = new StringBuilder();
            var parameters = new DynamicParameters();

            queryBuilder.AppendLine(@"SELECT P.Name, P.Alias, P.Occupation, P.Address, P.Email, P.AdmissionDate, P.Phone1, P.Phone2, 
                        CO.Name AS CountryName, P.Notes, P.IsActive, CP.Description AS CategoryDescription, P.CompanyId, 
                        C.Name AS ClientName
                        FROM PROVIDER P
                        JOIN PROVIDER_CATEGORY CP ON P.Id = CP.Id
                        JOIN COUNTRY CO ON P.IdCountry = CO.IdCountry
                        LEFT JOIN CLIENT C ON P.ClientId = C.ClientId
                        WHERE (@IsActive IS NULL OR P.IsActive = @IsActive)
                        AND ((@NameOrAlias IS NULL OR LOWER(P.Name) LIKE '%' + LOWER(@NameOrAlias) + '%')
                        OR (@NameOrAlias IS NULL OR LOWER(P.Alias) LIKE '%' + LOWER(@NameOrAlias) + '%'))
                        AND (@CountryId IS NULL OR P.IdCountry = @CountryId)
                        AND (@ClientId IS NULL OR P.ClientId = @ClientId)
                        AND (@CompanyId IS NULL OR P.CompanyId = @CompanyId)
                        AND CP.ProviderCategoryCode NOT IN ('PR','PROV', 'OCEANS', 'BONOS S')");

            parameters.Add("@IsActive", filtersAndPagination.Filters.IsActive, DbType.String);
            parameters.Add("@NameOrAlias", filtersAndPagination.Filters.NameOrAlias, DbType.String);
            parameters.Add("@CountryId", filtersAndPagination.Filters.CountryId, DbType.String);
            parameters.Add("@ClientId", filtersAndPagination.Filters.ClientId, DbType.Int32);
            parameters.Add("@CompanyId", filtersAndPagination.Filters.CompanyId, DbType.String);

            // Cuenta el número total de resultados sin aplicar la paginación
            var countQuery = "SELECT COUNT(*) FROM (" + queryBuilder.ToString() + ") AS TotalCountQuery;";
            var totalCount = await connection.ExecuteScalarAsync<int>(countQuery, parameters);

            // Aplica la paginación a la consulta
            queryBuilder.AppendLine("ORDER BY P.Name");
            queryBuilder.AppendLine("OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;");

            parameters.Add("@Skip", (filtersAndPagination.Pagination.PageIndex - 1) * filtersAndPagination.Pagination.PageSize, DbType.Int32);
            parameters.Add("@Take", filtersAndPagination.Pagination.PageSize, DbType.Int32);

            var results = await connection.QueryAsync<ProviderGetAllWithFiltersVM>(queryBuilder.ToString(), parameters);
            var providers = results.ToList();

            return (providers, totalCount);
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
                    existingProvider.Id = obj.Id;
                    return existingProvider.ProviderId;
                }
                return null;
            }
        }

    }
}
