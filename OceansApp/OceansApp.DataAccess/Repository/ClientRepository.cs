using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Clients;
using OceansApp.Models.ViewModels.Components;
using System.Data;
using System.Linq;

namespace OceansApp.DataAccess.Repository
{
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        private ApplicationDbContext _db;
        public ClientRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<(List<ClientsGetAllWithFiltersVM> clients, int totalCount)> GetAllClientsWithFiltersAsync(ClientsPaginationFiltersVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
            parameters.Add("@StartDate", filtersAndPagination.Filters.StartDate, DbType.Date);
            parameters.Add("@EndDate", filtersAndPagination.Filters.EndDate, DbType.Date);
            parameters.Add("@IsActive", filtersAndPagination.Filters.IsActive, DbType.String);
            parameters.Add("@CompanyId", filtersAndPagination.Filters.CompanyId, DbType.String);
            parameters.Add("@SuccessManagerId", filtersAndPagination.Filters.SuccessManagerId, DbType.Int32);
            parameters.Add("@FieldToOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.FieldToOrder, DbType.String);
            parameters.Add("@DirectionOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.DirectionOrder, DbType.String);
            parameters.Add("@Skip", (filtersAndPagination.PaginationWithoutFilters.Pagination.PageIndex - 1) * filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@Take", filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var results = await connection.QueryAsync<ClientsGetAllWithFiltersVM>("GetAllClientsWithFilters", parameters, commandType: CommandType.StoredProcedure);
            var totalCount = parameters.Get<int>("@TotalCount");

            var clients = results.ToList();

            return (clients, totalCount);
        }

        public async Task<List<GetDataForSelectVM>> GetAllClientsForSelectAsync()
        {
            var connection = _db.Database.GetDbConnection();
            var results = await connection.QueryAsync<GetDataForSelectVM>("SP_GetAllClientsForSelect", commandType: CommandType.StoredProcedure);
            return results.ToList();
        }
        public async Task<CreateUpdateClientVM> GetClientById(int clientId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ClientId", clientId);

            var client = await connection.QuerySingleOrDefaultAsync<CreateUpdateClientVM>("GetClientById", parameters, commandType: CommandType.StoredProcedure);

            return (client);
        }

        public void Update(Client obj)
        {
            _db.CLIENT.Update(obj);
        }

        public bool UpdateIfExistAddIfNot(Client obj)
        {
            var existingClient = GetFirstOrDefault(u => u.ClientCode == obj.ClientCode && u.CompanyId == obj.CompanyId);
            if (existingClient == null)
            {
                _db.CLIENT.Add(obj);
                _db.SaveChanges();
                return true;
            }
            else
            {
                if (existingClient.DateLastUpdate != obj.DateLastUpdate)
                {
                    //existingClient.Name = obj.Name;
                    //existingClient.Alias = obj.Alias;
                    //existingClient.Contact = obj.Contact;
                    //existingClient.ContactOccupation = obj.ContactOccupation;
                    //existingClient.Phone1 = obj.Phone1;
                    //existingClient.Phone2 = obj.Phone2;
                    //existingClient.AdmissionDate = obj.AdmissionDate;
                    //existingClient.PaymentCondition = obj.PaymentCondition;
                    //existingClient.Discount = obj.Discount;
                    //existingClient.IsActive = obj.IsActive;
                    //existingClient.ClientCategory = obj.ClientCategory;
                    //existingClient.ClientClass = obj.ClientClass;
                    //existingClient.Emails = obj.Emails;
                    //existingClient.Notes = obj.Notes;
                    //existingClient.DateLastUpdate = obj.DateLastUpdate;
                    //existingClient.Address = obj.Address;
                    //existingClient.CreationDate = obj.CreationDate;
                    //existingClient.CompanyId = obj.CompanyId;
                    return true;
                }
                return false;
            }
        }

    }
}
