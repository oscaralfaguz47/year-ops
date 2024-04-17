using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Blobs;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ReportingMyTime;
using OceansApp.Utility.SharedMethods.Blobs;


namespace OceansApp.DataAccess.Repository
{
    public class ReportingMyTimeMovementRepository : Repository<ReportingMyTimeMovement>, IReportingMyTimeMovementRepository
    {
        private ApplicationDbContext _db;
        public ReportingMyTimeMovementRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<MethodResponse> CreateReportingMyTimeMovementBlob(List<BlobUploadResult> uploadedBlobs, int movementId)
        {
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                var errorMessage = "";
                try
                {
                    List<string> uploadedBlobsNames = new List<string>();
                    foreach (var uploadedBlob in uploadedBlobs)
                    {
                        if (uploadedBlob.Success)
                        {
                            var blobToSave = new ReportingMyTimeMovementBlob()
                            {
                                MovementId = movementId,
                                BlobName = uploadedBlob.FileName,
                                ContainerId = uploadedBlob.ContainerId,
                                BlobUrl = uploadedBlob.BlobUrl,
                                Size = uploadedBlob.Size,
                                ContentType = uploadedBlob.ContentType,
                                CreationDate = uploadedBlob.UploadDate
                            };
                            _db.REPORTING_MY_TIME_MOVEMENT_BLOBS.Add(blobToSave);
                            uploadedBlobsNames.Add(uploadedBlob.FileName);
                        }
                        else
                        {
                            errorMessage += uploadedBlob.ErrorMessage + "/ ";
                        }
                    }
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return MethodResponse.CreateSuccessResponseStringsList($"{uploadedBlobsNames.Count} files were uploaded!", uploadedBlobsNames);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message + "/ " + errorMessage);
                }
            }
        }
        public async Task<List<IFormFile>> VerifyIfUploadFile(List<IFormFile> files, int movementId)
        {
            List<IFormFile> filesToUpload = new List<IFormFile>();
            CalculateContentHash calculateHash = new CalculateContentHash();
            foreach (var file in files)
            {
                string fileNameWithHass = $"{await calculateHash.CalculateContentHashAsync((IFormFile)file)}_{file.FileName}";
                var existingFile = await _db.REPORTING_MY_TIME_MOVEMENT_BLOBS.FirstOrDefaultAsync(x => x.BlobName == fileNameWithHass
                && x.MovementId == movementId);
                if (existingFile == null)
                {
                    filesToUpload.Add((IFormFile)file);
                }
            }
            return filesToUpload;
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

                    var movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.FirstOrDefaultAsync(x => x.Name == reportMovementData.MovementType);
                    if (movementType == null)
                    {
                        return MethodResponse.CreateFailureNotFoundResponse("Movement type not valid.");
                    }
                    var existingTimeMovement = await _db.REPORTING_MY_TIME_MOVEMENTS.FirstOrDefaultAsync(x => x.ActionDate == reportMovementData.ActionDate
                    && x.MovementTypeId == movementType.MovementTypeId && x.ProjectId == reportMovementData.ProjectId && x.ConsultantId == currentUser.ConsultantId);
                    if (existingTimeMovement != null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("There is a time movement with the same action date.");
                    }
                    var timeMovementToCreate = new ReportingMyTimeMovement
                    {
                        ConsultantId = currentUser.ConsultantId,
                        ProjectId = (int)reportMovementData.ProjectId,
                        Quantity = reportMovementData.Quantity == null ? 0 : (decimal)reportMovementData.Quantity,
                        ActionDate = (DateTime)reportMovementData.ActionDate,
                        Notes = reportMovementData.Notes,
                        TransactionStatusId = transactionStatus.TransactionStatusId,
                        MovementTypeId = movementType.MovementTypeId,
                        CreationDate = DateTime.UtcNow,
                    };
                    if (reportMovementData.MovementType == "Normal Hours"
                        || (reportMovementData.MovementType != "Normal Hours" && currentUser.ParticipatesInOnCalls))
                    {
                        await _db.REPORTING_MY_TIME_MOVEMENTS.AddAsync(timeMovementToCreate);
                        await _db.SaveChangesAsync();
                    }
                    await transaction.CommitAsync();

                    return MethodResponse.CreateSuccessResponse("Changes saved!", timeMovementToCreate.MovementId);
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
                        return MethodResponse.CreateFailureExceptionResponse("The movement does not exist.");
                    }
                    var currentUser = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userActionedBy);
                    if (existingTimeMovement.ConsultantId != currentUser.ConsultantId && existingTimeMovement.ProjectId != reportMovementData.ProjectId)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The provided movement does not belong to the current user.");
                    }

                    existingTimeMovement.Quantity = reportMovementData.Quantity == null ? 0 : (decimal)reportMovementData.Quantity;
                    existingTimeMovement.Notes = reportMovementData.Notes;
                    existingTimeMovement.LastUpdateDate = DateTime.UtcNow;

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return MethodResponse.CreateSuccessResponse("Changes saved!", existingTimeMovement.MovementId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                }
            }
        }

        public async Task<MethodResponse> DeleteTimeEntryClientNoTrackingTool(int movementId)
        {
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingTimeMovementToDelete = await _db.REPORTING_MY_TIME_MOVEMENTS.FirstOrDefaultAsync(x => x.MovementId == movementId);
                    if (existingTimeMovementToDelete == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The movement does not exist.");
                    }

                    _db.Remove(existingTimeMovementToDelete);

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return MethodResponse.CreateSuccessResponse("Changes saved!", null);
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
