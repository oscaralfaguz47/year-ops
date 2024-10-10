using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ProjecConsultantPendingSubmission;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IProjectConsultantPendingSubmissionRepository : IRepository<ProjectConsultantPendingSubmission> 
    {
        Task<List<ConsultantAndProjectVM>> GetConsultantsAndProjectsWhereSubmissionIsPendingAsync(DateTime startDate,
    DateTime endDate, int paymentPeriod);
        Task<MethodResponse> CreateProjectsConsultantsPendingSubmissionsAsync(DateTime startDate,
            DateTime endDate, int paymentPeriod);
        Task<List<ProjectsPendingSubmissionVM>> GetPendingProjectsPendingSubmissionByConsultantAsync(int consultantId,
            DateTime endDate);
    }
}
