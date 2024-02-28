using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantPositionRepository : IRepository<ConsultantPosition> 
    {
        Task<List<GetDataForSelectVM>> GetPositionsByIsAdministrative(bool isAdministrative);
        void Update(ConsultantPosition obj);
    }
}
