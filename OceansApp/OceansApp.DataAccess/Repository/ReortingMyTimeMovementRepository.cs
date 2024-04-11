using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ReportingMyTime;


namespace OceansApp.DataAccess.Repository
{
    public class ReportingMyTimeMovementRepository : Repository<ReportingMyTimeMovement>, IReportingMyTimeMovementRepository
    {
        private ApplicationDbContext _db;
        public ReportingMyTimeMovementRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<MethodResponse> CreateTimeEntryClientNoTrackingTool(string userIdCreatedBy, CreateUpdateMovementClientNoTrackingToolVM reportMovementData)
        {
            if (reportMovementData == null)
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

                    if (!await _db.PROJECTS_CONSULTANTS_ASSIGNED.AnyAsync(x => x.ProjectId == reportMovementData.ProjectId && x.ConsultantId == currentUser.ConsultantId))
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The user is not assigned to the provided project.");
                    }

                    var project = await _db.PROJECTS.FirstOrDefaultAsync(x => x.ProjectId == reportMovementData.ProjectId);
                    if (project == null || !project.ClientHasTrackingTool)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Invalid project configuration.");
                    }

                    var transactionStatus = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "No actions");
                    if (transactionStatus == null)
                    {
                        return MethodResponse.CreateFailureNotFoundResponse("Transaction status 'No actions' not found.");
                    }
                    var movementTypeNormalHours = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.FirstOrDefaultAsync(x => x.Name == "Normal Hours");
                    if (movementTypeNormalHours == null)
                    {
                        return MethodResponse.CreateFailureNotFoundResponse("Movement type not found.");
                    }
                    var existingTimeMovement = await _db.REPORTING_MY_TIME_MOVEMENTS.FirstOrDefaultAsync(x => x.ActionDate == reportMovementData.ActionDate
                    && x.MovementTypeId == movementTypeNormalHours.MovementTypeId && x.ProjectId == reportMovementData.ProjectId && x.ConsultantId == currentUser.ConsultantId);
                    if (existingTimeMovement != null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("There is a time movement with the same action date.");
                    }
                    var timeMovementToCreate = new ReportingMyTimeMovement
                    {
                        ConsultantId = currentUser.ConsultantId,
                        ProjectId = (int)reportMovementData.ProjectId,
                        Quantity = (decimal)reportMovementData.Quantity,
                        ActionDate = (DateTime)reportMovementData.ActionDate,
                        Notes = reportMovementData.Notes,
                        TransactionStatusId = transactionStatus.TransactionStatusId,
                        MovementTypeId = movementTypeNormalHours.MovementTypeId,
                        CreationDate = DateTime.UtcNow,
                    };

                    await _db.REPORTING_MY_TIME_MOVEMENTS.AddAsync(timeMovementToCreate);
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return MethodResponse.CreateSuccessResponse("Changes successfully saved!",timeMovementToCreate.MovementId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                }
            }
        }

        public async Task<MethodResponse> UpdateTimeEntryClientNoTrackingTool(string userActionedBy,
            CreateUpdateMovementClientNoTrackingToolVM reportMovementData)
        {
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingTimeMovement = await _db.REPORTING_MY_TIME_MOVEMENTS.FirstOrDefaultAsync(x => x.MovementId == reportMovementData.MovementId);
                    if (existingTimeMovement == null)
                    {
                        _ = CreateTimeEntryClientNoTrackingTool(userActionedBy, reportMovementData);
                    }
                    var currentUser = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userActionedBy);
                    if (existingTimeMovement.ConsultantId != currentUser.ConsultantId && existingTimeMovement.ProjectId != reportMovementData.ProjectId)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The provided movement does not belong to the current user.");
                    }

                    existingTimeMovement.Quantity = (decimal)reportMovementData.Quantity;
                    existingTimeMovement.Notes = reportMovementData.Notes;
                    existingTimeMovement.LastUpdateDate = DateTime.UtcNow;

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return MethodResponse.CreateSuccessResponse("Changes successfully saved!", existingTimeMovement.MovementId);
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
