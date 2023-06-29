using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantRoleRepository : IRepository<ConsultantRole> 
    {
        void Update(ConsultantRole obj);
    }
}
