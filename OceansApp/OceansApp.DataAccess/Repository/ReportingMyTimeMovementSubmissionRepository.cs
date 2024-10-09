using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ReportingMyTimeSubmissions;


namespace OceansApp.DataAccess.Repository
{
    public class ReportingMyTimeMovementSubmissionRepository : Repository<ReportingMyTimeMovementSubmission>, IReportingMyTimeMovementSubmissionRepository
    {
        private ApplicationDbContext _db;
        public ReportingMyTimeMovementSubmissionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<MethodResponse> CreateSubmission(string userIdCreatedBy, CreateSubmissionVM submissionData)
        {
            if (submissionData == null)
            {
                return MethodResponse.CreateFailureValidationResponse("Report movement data cannot be null.");
            }
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var currentUser = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userIdCreatedBy);
                    if (currentUser == null)
                    {
                        return MethodResponse.CreateFailureNotFoundResponse("Consultant not found.");
                    }

                    if (!await _db.PROJECTS_CONSULTANTS_ASSIGNED.AnyAsync(x => x.ProjectId == submissionData.ProjectId && x.ConsultantId == currentUser.ConsultantId))
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The user is not assigned to the provided project.");
                    }

                    var project = await _db.PROJECTS.FirstOrDefaultAsync(x => x.ProjectId == submissionData.ProjectId);
                    if (project == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Invalid project configuration.");
                    }

                    var movement = await _db.REPORTING_MY_TIME_MOVEMENTS.FirstOrDefaultAsync(x => x.ProjectId == project.ProjectId &&
                    x.ConsultantId == currentUser.ConsultantId && x.Quantity > 0 && (x.ActionDate.Date >= submissionData.StartPeriodDate.Date &&
                    x.ActionDate.Date <= submissionData.EndPeriodDate.Date));

                    if (movement == null)
                    {
                        return MethodResponse.CreateFailureValidationResponse("Enter and save your worked hours to submit the report.", "Hours");
                    }
                    if (project.ClientHasTrackingTool)
                    {
                        var blobMovement = await _db.REPORTING_MY_TIME_MOVEMENTS.FirstOrDefaultAsync(x => x.ProjectId == project.ProjectId &&
                    x.ConsultantId == currentUser.ConsultantId && (x.ActionDate.Date >= submissionData.StartPeriodDate.Date &&
                    x.ActionDate.Date <= submissionData.EndPeriodDate.Date));
                        if (blobMovement == null)
                        {
                            return MethodResponse.CreateFailureValidationResponse("Enter and save your worked hours to submit the report.", "Hours");
                        }
                        int uploadedBlobs = await _db.REPORTING_MY_TIME_MOVEMENT_BLOBS.CountAsync(x => x.MovementId == blobMovement.MovementId);
                        if (uploadedBlobs == 0)
                        {
                            return MethodResponse.CreateFailureValidationResponse("You must upload at least one file to submit the report.", "Report");
                        }
                    }

                    var transactionStatusWaitingToBeApproved = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Waiting to be approved");
                    if (transactionStatusWaitingToBeApproved == null)
                    {
                        return MethodResponse.CreateFailureNotFoundResponse("Transaction status 'Waiting to be approved' not found.");
                    }

                    var movements = await _db.REPORTING_MY_TIME_MOVEMENTS.Where(x => x.ProjectId == project.ProjectId &&
                    x.ConsultantId == currentUser.ConsultantId && (x.ActionDate.Date >= submissionData.StartPeriodDate.Date &&
                    x.ActionDate.Date <= submissionData.EndPeriodDate.Date) && (x.TransactionStatus.Name == "No actions" 
                    || x.TransactionStatus.Name == "Rejected"))
                        .Include(x => x.TransactionStatus).ToListAsync();

                    var existingSubmission = await _db.REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS.Include(x => x.TransactionStatus).FirstOrDefaultAsync(x => 
                    x.ConsultantId == currentUser.ConsultantId && x.ProjectId == submissionData.ProjectId && x.StartPeriodDate.Date == submissionData.StartPeriodDate.Date 
                    && x.EndPeriodDate.Date == submissionData.EndPeriodDate.Date);

                    if (existingSubmission == null)
                    {
                        foreach (var repMovement in movements)
                        {
                            repMovement.TransactionStatusId = transactionStatusWaitingToBeApproved.TransactionStatusId;
                        }

                        var submissionToCreate = new ReportingMyTimeMovementSubmission
                        {
                            ConsultantId = currentUser.ConsultantId,
                            ProjectId = (int)submissionData.ProjectId,
                            TransactionStatusId = transactionStatusWaitingToBeApproved.TransactionStatusId,
                            SubmissionDate = DateTime.UtcNow,
                            StartPeriodDate = submissionData.StartPeriodDate,
                            EndPeriodDate = submissionData.EndPeriodDate
                        };

                        await _db.REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS.AddAsync(submissionToCreate);
                        await _db.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return MethodResponse.CreateSuccessResponse("You have submitted your report!");
                    }
                    else
                    {
                        if (existingSubmission.TransactionStatus.Name == "Rejected")
                        {
                            foreach (var repMovement in movements)
                            {
                                repMovement.TransactionStatusId = transactionStatusWaitingToBeApproved.TransactionStatusId;
                            }

                            existingSubmission.LastSubmissionDate = DateTime.UtcNow;
                            existingSubmission.TransactionStatusId = transactionStatusWaitingToBeApproved.TransactionStatusId;
                            await _db.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return MethodResponse.CreateSuccessResponse("You have re-submitted your report!");
                        }
                        else
                        {
                            await transaction.RollbackAsync();
                            return MethodResponse.CreateFailureExceptionResponse("No actions applied. Your report is already submitted!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                }
            }
        }

        public async Task<List<PendingSubmissionsVM>> GetPendingTimesheetsSubmissionsAsync(int consultantId)
        {
            List<PendingSubmissionsVM> listToReturn = new();



            return listToReturn;
        }

    }
}
