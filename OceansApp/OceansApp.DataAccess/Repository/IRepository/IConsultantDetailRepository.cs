using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Consultants;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantDetailRepository : IRepository<ConsultantDetail> 
    {
        Task<List<GetUsersSelectVM>> GetUsersByCategoryAndPositionForSelect(string userCategory, string userPosition);
        Task<int> GetNumOfUsersByCategoryConsultantIdAndPosition(string userCategory, string userPosition, int consultantId);
        Task<List<GetConsultantsBySearchTextVM>> GetConsultantsBySearchText(string searchText);
        void Update(ConsultantDetail obj);
    }
}
