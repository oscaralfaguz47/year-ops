using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.ConsultantRolesQualityLevels;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantRoleQualityLevelRepository : IRepository<ConsultantRolesQualityLevels> 
    {
        void Update(ConsultantRolesQualityLevels obj);
        IEnumerable<GetConsultantRolesQualityLevelsVM> GetConsultantRoleQualityLevelsList();
    }
}
