
using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IClientRepository : IRepository<Client> 
    {
        void Update(Client obj);
        public bool UpdateIfExistAddIfNot(Client obj);
    }
}
