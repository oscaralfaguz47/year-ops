
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Clients;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IClientRepository : IRepository<Client> 
    {
        Task<(List<ClientsGetAllWithFiltersVM> clients, int totalCount)> GetAllClientsWithFiltersAsync(ClientsPaginationFiltersVM filtersAndPagination);
        void Update(Client obj);
        public bool UpdateIfExistAddIfNot(Client obj);
    }
}
