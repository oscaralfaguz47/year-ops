using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ReportingMyTime;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IReportingMyTimeMovementRepository : IRepository<ReportingMyTimeMovement> 
    {
        Task<MethodResponse> CreateTimeEntryClientNoTrackingTool(string userIdCreatedBy,
            CreateUpdateMovementClientNoTrackingToolVM reportMovementData);
        Task<MethodResponse> UpdateTimeEntryClientNoTrackingTool(string userActionedBy,
            CreateUpdateMovementClientNoTrackingToolVM reportMovementData);
    }
}
