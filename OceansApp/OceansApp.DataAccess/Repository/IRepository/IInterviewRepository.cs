using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Interviews;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IInterviewRepository : IRepository<Interview>
    {
        Task<(List<InterviewsGetAllWithFiltersVM> interviews, int totalCount)> GetAllInterviewsWithFiltersAsync(InterviewsPaginationFiltersVM filtersAndPagination);
        Task<MethodResponse> CreateInterview(string userIdCreatedBy,
            CreateUpdateInterviewVM interviewData);
        Task<MethodResponse> UpdateInterview(string userActionedBy, CreateUpdateInterviewVM interviewData);
        Task<CreateUpdateInterviewVM> GetInterviewDataById(int interviewId);
        Task<MethodResponse> RejectInterview(string userActionedBy, int interviewId);
    }
}
