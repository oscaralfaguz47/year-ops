using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantPositionRepository : IRepository<ConsultantPosition> 
    {
        void Update(ConsultantPosition obj);
    }
}
