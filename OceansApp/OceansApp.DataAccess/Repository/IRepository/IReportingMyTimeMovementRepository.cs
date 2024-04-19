using Microsoft.AspNetCore.Http;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Blobs;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ReportingMyTime;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IReportingMyTimeMovementRepository : IRepository<ReportingMyTimeMovement> 
    {
        Task<List<GetProjectMovementsVM>> GetProjectMovementsAsync(int projectId, int consultId, DateTime startDate,
            DateTime endDate);
        Task<MethodResponse> CreateReportingMyTimeMovementBlob(List<BlobUploadResult> uploadedBlobs, int movementId);
        Task<MethodResponse> CreateTimeEntryClientNoTrackingTool(string userIdCreatedBy,
            CreateUpdateMovementClientNoTrackingToolVM reportMovementData);
        Task<MethodResponse> UpdateTimeEntryClientNoTrackingTool(string userActionedBy,
            CreateUpdateMovementClientNoTrackingToolVM reportMovementData);
        Task<MethodResponse> DeleteTimeEntryClientNoTrackingTool(int movementId);
        Task<List<IFormFile>> VerifyIfUploadFile(List<IFormFile> files, int movementId);
        Task<MethodResponse> GetExistingMovement(string userIdCreatedBy, CreateUpdateMovementClientNoTrackingToolVM reportMovementData);
        Task<MethodResponse> DeleteBlobReport(string fileName);

        Task<MethodResponse> CreateTimeEntryTrackingTool(string userIdCreatedBy,
            CreateUpdateMovementTrackingToolVM timeEntryData);
        Task<MethodResponse> UpdateTimeEntryTrackingTool(string userActionedBy,
           CreateUpdateMovementTrackingToolVM timeEntryData);
        Task<List<GetTrackingToolProjectMovementsVM>> GetTrackingToolProjectMovementsAsync(int projectId, int consultId, DateTime startDate,
             DateTime endDate);
    }
}
