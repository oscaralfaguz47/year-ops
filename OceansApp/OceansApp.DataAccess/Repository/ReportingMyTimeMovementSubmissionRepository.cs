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
                    x.ConsultantId == currentUser.ConsultantId && x.Quantity > 0 && (x.ActionDate >= submissionData.StartPeriodDate &&
                    x.ActionDate <= submissionData.EndPeriodDate));
                    if (movement == null)
                    {
                        return MethodResponse.CreateFailureValidationResponse("Enter and save your worked hours to submit the report.", "Hours");
                    }
                    if (project.ClientHasTrackingTool)
                    {
                        var blobMovement = await _db.REPORTING_MY_TIME_MOVEMENTS.FirstOrDefaultAsync(x => x.ProjectId == project.ProjectId &&
                    x.ConsultantId == currentUser.ConsultantId && (x.ActionDate >= submissionData.StartPeriodDate &&
                    x.ActionDate <= submissionData.EndPeriodDate));
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
                    var transactionStatusApproved = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "Approved");
                    if (transactionStatusApproved == null)
                    {
                        return MethodResponse.CreateFailureNotFoundResponse("Transaction status 'Approved' not found.");
                    }
                    var transactionStatusNoActions = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "No actions");
                    if (transactionStatusNoActions == null)
                    {
                        return MethodResponse.CreateFailureNotFoundResponse("Transaction status 'No actions' not found.");
                    }
                    var consultantBenefits = await _db.CONSULTANT_REIMBURSED_BENEFITS.Where(x=>x.ConsultantId == currentUser.ConsultantId &&
                     (x.DateToBeReimbursed >= submissionData.StartPeriodDate && x.DateToBeReimbursed <= submissionData.EndPeriodDate) && 
                     x.TransactionStatusId == transactionStatusApproved.TransactionStatusId).ToListAsync();

                    var movements = await _db.REPORTING_MY_TIME_MOVEMENTS.Where(x => x.ProjectId == project.ProjectId &&
                    x.ConsultantId == currentUser.ConsultantId && (x.ActionDate >= submissionData.StartPeriodDate &&
                    x.ActionDate <= submissionData.EndPeriodDate) && x.TransactionStatusId == transactionStatusNoActions.TransactionStatusId).ToListAsync();

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
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                }
            }
        }

    }
}
