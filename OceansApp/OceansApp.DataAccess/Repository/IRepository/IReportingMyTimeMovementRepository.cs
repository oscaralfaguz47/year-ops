using Microsoft.AspNetCore.Http;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Blobs;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ReportingMyTime;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IReportingMyTimeMovementRepository : IRepository<ReportingMyTimeMovement> 
    {
        Task<MethodResponse> CreateReportingMyTimeMovementBlob(string containerId, List<BlobUploadResult> uploadedBlobs, int movementId);
        Task<MethodResponse> CreateTimeEntryClientNoTrackingTool(string userIdCreatedBy,
            CreateUpdateMovementClientNoTrackingToolVM reportMovementData);
        Task<MethodResponse> UpdateTimeEntryClientNoTrackingTool(string userActionedBy,
            CreateUpdateMovementClientNoTrackingToolVM reportMovementData);
        Task<MethodResponse> DeleteTimeEntryClientNoTrackingTool(int movementId);
    }
}
