using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Consultants;
using System.Linq.Expressions;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IConsultantDetailRepository : IRepository<ConsultantDetail> 
    {
        Task<(List<ConsultantsGetAllWithFiltersVM> consultants, int totalCount)> GetAllConsultantsWithFiltersAsync(ConsultantsPaginationFiltersVM filtersAndPagination);
        Task<List<GetUsersSelectVM>> GetUsersByCategoryAndPositionForSelect(string userCategory, string userPosition);
        Task<int> GetNumOfUsersByCategoryConsultantIdAndPosition(string userCategory, string userPosition, int consultantId);
        Task<List<GetConsultantsBySearchTextVM>> GetConsultantsBySearchText(string? searchText, string? userCategoryName);
        Task<MethodResponse> CreateConsultant(string createdUserId, string userIdCreatedBy, CreateUpdateConsultantVM consultantData);
        Task<MethodResponse> UpdateUserConsultant(string userActionedBy, CreateUpdateConsultantVM consultantData, bool isAuthForManageAdminUsers);
        Task<CreateUpdateConsultantVM> GetConsultantDataById(int consultantId);
        void Update(ConsultantDetail obj);
    }
}
