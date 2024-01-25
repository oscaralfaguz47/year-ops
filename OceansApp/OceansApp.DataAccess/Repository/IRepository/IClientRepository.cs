
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Clients;
using OceansApp.Models.ViewModels.Components;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IClientRepository : IRepository<Client> 
    {
        Task<(List<ClientsGetAllWithFiltersVM> clients, int totalCount)> GetAllClientsWithFiltersAsync(ClientsPaginationFiltersVM filtersAndPagination);
        Task<List<GetDataForSelectVM>> GetAllClientsForSelectAsync();
        Task<CreateUpdateClientVM> GetClientById(int clientId);
        void Update(Client obj);
        public bool UpdateIfExistAddIfNot(Client obj);
    }
}
