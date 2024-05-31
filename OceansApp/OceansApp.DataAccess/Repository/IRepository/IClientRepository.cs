
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Clients;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Consultants;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IClientRepository : IRepository<Client> 
    {
        Task<(List<ClientsGetAllWithFiltersVM> clients, int totalCount)> GetAllClientsWithFiltersAsync(ClientsPaginationFiltersVM filtersAndPagination);
        Task<List<GetDataForSelectVM>> GetAllClientsForSelectAsync();
        Task<CreateUpdateClientVM> GetClientById(int clientId);
        Task<GetUsersSelectVM> GetSuccessManagerIdAndNameByClientId(int clientId);
        void Update(Client obj);
        Task<bool> UpdateIfExistAddIfNot(Client obj);
    }
}
