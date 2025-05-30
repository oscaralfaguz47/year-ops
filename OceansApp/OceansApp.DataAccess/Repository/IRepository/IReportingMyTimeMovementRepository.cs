using Microsoft.AspNetCore.Http;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Blobs;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ReportingMyTime;
using OceansApp.Models.ViewModels.ReportingMyTime.Reports;
using OceansApp.Models.ViewModels.ReportingMyTimeMovements;
using System.Linq.Expressions;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IReportingMyTimeMovementRepository : IRepository<ReportingMyTimeMovement> 
    {
        Task<ReportingMyTimeMovement?> GetFirstOrDefaultAsync(
    Expression<Func<ReportingMyTimeMovement, bool>>? predicate,
    params Expression<Func<ReportingMyTimeMovement, object>>[] includes);
        Task<List<GetProjectMovementsVM>> GetProjectMovementsAsync(int projectId, int consultId, DateTime startDate,
            DateTime endDate);
        Task<MethodResponse> CreateReportingMyTimeMovementBlob(List<BlobUploadResult> uploadedBlobs, int movementId);
        Task<MethodResponse> CreateTimeEntryClientNoTrackingTool(string userIdCreatedBy,
            CreateUpdateMovementClientNoTrackingToolVM reportMovementData);
        Task<MethodResponse> UpdateTimeEntryClientNoTrackingTool(string userActionedBy,
            CreateUpdateMovementClientNoTrackingToolVM reportMovementData);
        Task<MethodResponse> DeleteTimeEntryClientNoTrackingTool(int movementId);
        Task<List<IFormFile>> VerifyIfUploadFile(List<IFormFile> files, int movementId);
        Task<int?> VerifyNumUploadedFilesPerMovementAsync(int movementId);
        Task<MethodResponse> GetExistingMovement(string userIdCreatedBy, CreateUpdateMovementClientNoTrackingToolVM reportMovementData);
        Task<MethodResponse> DeleteBlobReport(string fileName);

        Task<MethodResponse> CreateTimeEntryTrackingTool(string userIdCreatedBy,
            CreateUpdateMovementTrackingToolVM timeEntryData);
        Task<MethodResponse> AutofillTimeEntryTrackingTool(string userIdCreatedBy,
            CreateUpdateMovementTrackingToolVM timeEntryData, DateTime startDate, DateTime endDate);
        Task<MethodResponse> UpdateTimeEntryTrackingTool(string userActionedBy,
           CreateUpdateMovementTrackingToolVM timeEntryData);
        Task<List<GetTrackingToolProjectMovementsVM>> GetTrackingToolProjectMovementsAsync(int projectId, int consultId, DateTime startDate,
             DateTime endDate);
        Task<MethodResponse> DeleteTrackingTooTimeEntry(string userActionedBy, int movementId);
        Task<MethodResponse> ValidateSubmission(ReportingMyTimeMovement? movement, DateTime? actionDate,
            ConsultantDetail? consultant, int? projectId);
        Task<List<GetApprovedMovementsWhereConsultantVM>> GetApprovedMovementsWhereConsultant(int consultantId,
            int projectId, DateTime startDate, DateTime endDate);
        Task<List<GlobalHoursReport>> GetGlobalMovementsWithFiltersAsync(
    DateTime startDate,
    DateTime endDate,
    int? movementTypeId,
    IEnumerable<int>? projectIds,
    IEnumerable<int>? clientIds,
    IEnumerable<int>? consultantIds);

        Task<List<GetBillableHoursForBillingVM>> GetBillableHoursForBillingAsync(int clientId, DateTime startDate,
            DateTime endDate);
    }
}
