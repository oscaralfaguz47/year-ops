
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ReportingMyTimeSubmissions;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IReportingMyTimeMovementSubmissionRepository : IRepository<ReportingMyTimeMovementSubmission> 
    {
        Task<MethodResponse> CreateSubmission(string userIdCreatedBy, CreateSubmissionVM submissionData);
    }
}
