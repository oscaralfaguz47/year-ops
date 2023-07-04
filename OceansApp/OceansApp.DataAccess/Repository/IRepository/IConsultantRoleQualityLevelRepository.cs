using OceansApp.Models.Models;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantRoleQualityLevelRepository : IRepository<ConsultantRolesQualityLevels> 
    {
        void Update(ConsultantRolesQualityLevels obj);
    }
}
