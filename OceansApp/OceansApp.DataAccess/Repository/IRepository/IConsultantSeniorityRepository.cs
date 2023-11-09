using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantSeniorityRepository : IRepository<ConsultantSeniority> 
    {
        void Update(ConsultantSeniority obj);
    }
}
