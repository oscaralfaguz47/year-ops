using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantQualityLevelRepository : IRepository<ConsultantQualityLevel> 
    {
        void Update(ConsultantQualityLevel obj);
    }
}
