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
        private readonly IProjectConsultantAssignedHistoryRepository _projectConsultantAssignedHistoryRepository;
        private readonly IHourReportValidationServiceRepository _hourReportValidationServiceRepository;
        public ReportingMyTimeMovementSubmissionRepository(ApplicationDbContext db, IUnitOfWork unitOfWork, IHourReportValidationServiceRepository hourReportValidationServiceRepository) : base(db)
        {
            _db = db;
            _projectConsultantAssignedHistoryRepository = unitOfWork.ProjectConsultantAssignedHistory;
            _hourReportValidationServiceRepository = hourReportValidationServiceRepository;
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
                        //File validations
                        var currentProjectConsultantHistory = await _projectConsultantAssignedHistoryRepository.GetCurrentProjectConsultantHistoryAsync(currentUser.ConsultantId, submissionData.ProjectId, submissionData.EndPeriodDate);
                        if (currentProjectConsultantHistory == null)
                        {
                            return MethodResponse.CreateFailureNotFoundResponse("The consultant doesn't have a project history.");
                        }

                        int uploadedBlobsPrimaryTrackingTool = await _db.REPORTING_MY_TIME_MOVEMENT_BLOBS.CountAsync(x => x.MovementId == blobMovement.MovementId &&
                        x.PrimaryReportTrackingToolName.Trim() == currentProjectConsultantHistory.PrimaryReportTrackingToolName.Trim());
                        if (uploadedBlobsPrimaryTrackingTool == 0)
                        {
                            return MethodResponse.CreateFailureValidationResponse($"At least one file from <strong>'{currentProjectConsultantHistory.PrimaryReportTrackingToolName.Trim()}'</strong> is required to submit.", "Report");
                        }
                        //Second file validations

                        if (currentProjectConsultantHistory.SecondReportTrackingToolName != null)
                        {
                            int uploadedBlobsSecondTrackingTool = await _db.REPORTING_MY_TIME_MOVEMENT_BLOBS.CountAsync(x => x.MovementId == blobMovement.MovementId &&
                            x.SecondReportTrackingToolName.Trim() == currentProjectConsultantHistory.SecondReportTrackingToolName.Trim());
                            if (uploadedBlobsSecondTrackingTool == 0)
                            {
                                return MethodResponse.CreateFailureValidationResponse($"At least one file from <strong>'{currentProjectConsultantHistory.SecondReportTrackingToolName.Trim()}'</strong> is required to submit.", "Report");
                            }

                            //Validate with AI
                            //                        if (!string.IsNullOrWhiteSpace(currentProjectConsultantHistory.PrimaryReportTrackingToolName) &&
                            //!string.IsNullOrWhiteSpace(currentProjectConsultantHistory.SecondReportTrackingToolName) && (submissionData.ConfirmSubmitWithDifferences == false 
                            //|| submissionData.ConfirmSubmitWithDifferences == null))
                            //                        {
                            //                            var (isValid, message) = await _hourReportValidationServiceRepository.ValidateMatchingReportsAsync(
                            //                                movement.MovementId,
                            //                                currentProjectConsultantHistory.PrimaryReportTrackingToolName,
                            //                                currentProjectConsultantHistory.SecondReportTrackingToolName,
                            //                                submissionData.StartPeriodDate, submissionData.EndPeriodDate
                            //                            );

                            //                            if (!isValid)
                            //                            {
                            //                                return MethodResponse.CreateFailureValidationResponse(message, "OpenAI");
                            //                            }
                            //                        }

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

        public async Task<List<LastTimesheetSubmittedVM>> GetLastTimesheetSubmittedAsync(int consultantId)
        {
            var result = (from su in _db.REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS
                          join p in _db.PROJECTS on su.ProjectId equals p.ProjectId
                          join ts in _db.TRANSACTION_STATUSES on su.TransactionStatusId equals ts.TransactionStatusId
                          where su.ConsultantId == consultantId
                          orderby su.SubmissionDate descending
                          select new LastTimesheetSubmittedVM
                          {
                              StartDate = su.StartPeriodDate,
                              EndDate = su.EndPeriodDate,
                              ProjectName = p.Name,
                              Status = ts.Name,
                              TotalHours = _db.REPORTING_MY_TIME_MOVEMENTS
                                             .Where(rmtm => rmtm.ProjectId == su.ProjectId
                                                         && rmtm.ConsultantId == su.ConsultantId
                                                         && rmtm.ActionDate >= su.StartPeriodDate
                                                         && rmtm.ActionDate <= su.EndPeriodDate)
                                             .Sum(rmtm => (decimal?)rmtm.Quantity) ?? 0
                          })
             .Take(10)
             .ToList();

            return result;

        }

    }
}
