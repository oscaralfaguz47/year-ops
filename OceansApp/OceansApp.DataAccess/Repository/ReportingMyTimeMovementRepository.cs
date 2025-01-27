using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Blobs;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ReportingMyTime;
using OceansApp.Models.ViewModels.ReportingMyTime.Reports;
using OceansApp.Utility.SharedMethods;
using OceansApp.Utility.SharedMethods.Blobs;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using static OceansApp.Models.ViewModels.Components.MethodResponse;


namespace OceansApp.DataAccess.Repository
{
    public class ReportingMyTimeMovementRepository : Repository<ReportingMyTimeMovement>, IReportingMyTimeMovementRepository
    {
        private ApplicationDbContext _db;
        public ReportingMyTimeMovementRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        //COMMON METHODS
        public async Task<ReportingMyTimeMovement?> GetFirstOrDefaultAsync(
    Expression<Func<ReportingMyTimeMovement, bool>>? predicate,
    params Expression<Func<ReportingMyTimeMovement, object>>[] includes)
        {
            IQueryable<ReportingMyTimeMovement> query = _db.REPORTING_MY_TIME_MOVEMENTS.AsQueryable();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            try
            {
                var movement = await query.FirstOrDefaultAsync();
                return movement;
            }
            catch (DbUpdateException ex)
            {
                throw;
            }
        }

        // CLIENT HAS TRACKING TOOL - METHODS
        public async Task<List<GetProjectMovementsVM>> GetProjectMovementsAsync(int projectId, int consultId, DateTime startDate,
            DateTime endDate)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@ProjectId", projectId, DbType.Int32);
            parameters.Add("@ConsultantId", consultId, DbType.Int32);
            parameters.Add("@StartActionDate", startDate, DbType.Date);
            parameters.Add("@FinalActionDate", endDate, DbType.Date);

            var results = await connection.QueryAsync<GetProjectMovementsVM>("SP_REPORTING_MY_TIME_GetProjectMovements",
                parameters, commandType: CommandType.StoredProcedure);
            var movements = results.ToList();

            return movements;
        }
        public async Task<MethodResponse> GetExistingMovement(string userIdCreatedBy, CreateUpdateMovementClientNoTrackingToolVM reportMovementData)
        {
            try
            {
                var currentUser = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userIdCreatedBy);
                if (currentUser == null)
                {
                    return MethodResponse.CreateFailureNotFoundResponse("Consultant not found.");
                }

                int? movementId = null;
                var existingMovements = await _db.REPORTING_MY_TIME_MOVEMENTS.FirstOrDefaultAsync(x => x.ActionDate >= reportMovementData.StartActionDate
                && x.ActionDate <= reportMovementData.ActionDate && x.ProjectId == reportMovementData.ProjectId &&
                x.ConsultantId == currentUser.ConsultantId && x.MovementTypeId == reportMovementData.MovementTypeId);

                if (existingMovements != null)
                {
                    movementId = existingMovements.MovementId;
                }
                return MethodResponse.CreateSuccessResponse(null, movementId);
            }
            catch (Exception ex)
            {
                return MethodResponse.CreateFailureExceptionResponse(ex.Message);
            }
        }
        public async Task<MethodResponse> CreateReportingMyTimeMovementBlob(List<BlobUploadResult> uploadedBlobs, int movementId)
        {
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                var errorMessage = "";
                string blobName = "";
                try
                {
                    var objectList = new List<AzureUploadedFilesVM>();

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
                            AzureUploadedFilesVM uploadedFile = new AzureUploadedFilesVM()
                            {
                                BlobUrl = uploadedBlob.BlobUrl,
                                BlobName = uploadedBlob.FileName
                            };
                            objectList.Add(uploadedFile);
                            blobName = uploadedBlob.FileName;
                        }
                        else
                        {
                            errorMessage += uploadedBlob.ErrorMessage;
                            return MethodResponse.CreateFailureExceptionResponse(errorMessage);
                        }
                    }
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return MethodResponse.CreateSuccessResponseAnyList($"The file ({RemoveIdToBlobNames.RemoveId(blobName)}) was uploaded!", objectList);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                }
            }
        }
        public async Task<List<IFormFile>> VerifyIfUploadFile(List<IFormFile> files, int movementId)
        {
            List<IFormFile> filesToUpload = new List<IFormFile>();
            CalculateContentHash calculateHash = new CalculateContentHash();
            foreach (var file in files)
            {
                string fileNameWithHass = $"{await calculateHash.CalculateContentHashAsync((IFormFile)file)}_{movementId}_{file.FileName}";
                var existingFile = await _db.REPORTING_MY_TIME_MOVEMENT_BLOBS.FirstOrDefaultAsync(x => x.BlobName == fileNameWithHass
                && x.MovementId == movementId);
                if (existingFile == null)
                {
                    filesToUpload.Add((IFormFile)file);
                }
            }
            return filesToUpload;
        }
        public async Task<int?> VerifyNumUploadedFilesPerMovementAsync(int movementId)
        {
            try
            {
                return await _db.REPORTING_MY_TIME_MOVEMENT_BLOBS.CountAsync(x => x.MovementId == movementId);
            }
            catch
            {
                return null;
            }
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

                    var transactionStatusNoActions = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "No actions");
                    if (transactionStatusNoActions == null)
                    {
                        return MethodResponse.CreateFailureNotFoundResponse("Transaction status 'No actions' not found.");
                    }

                    MethodResponse responseValidateSubmission = await ValidateSubmission(null, reportMovementData.ActionDate, currentUser,
                        reportMovementData.ProjectId);
                    if (!responseValidateSubmission.Success)
                    {
                        return MethodResponse.CreateFailureExceptionResponse(responseValidateSubmission.Message);
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
                        TransactionStatusId = transactionStatusNoActions.TransactionStatusId,
                        MovementTypeId = movementType.MovementTypeId,
                        CreationDate = DateTime.UtcNow,
                    };
                    if (reportMovementData.MovementType == "Normal Hours"
                        || (reportMovementData.MovementType != "Normal Hours")) //&& currentUser.ParticipatesInOnCalls
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
                    if (existingTimeMovement.ConsultantId != currentUser.ConsultantId || existingTimeMovement.ProjectId != reportMovementData.ProjectId)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The provided movement does not belong to the current user.");
                    }

                    MethodResponse responseValidateSubmission = await ValidateSubmission(null, reportMovementData.ActionDate, currentUser,
                        reportMovementData.ProjectId);
                    if (!responseValidateSubmission.Success)
                    {
                        return MethodResponse.CreateFailureExceptionResponse(responseValidateSubmission.Message);
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
        public async Task<MethodResponse> DeleteBlobReport(string fileName)
        {
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingTimeMovement = await _db.REPORTING_MY_TIME_MOVEMENT_BLOBS.FirstOrDefaultAsync(x => x.BlobName == fileName);
                    if (existingTimeMovement == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The file does not exist.");
                    }
                    _db.Remove(existingTimeMovement);
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return MethodResponse.CreateSuccessResponse("File deleted!");
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
                    var existingTimeMovementToDelete = await _db.REPORTING_MY_TIME_MOVEMENTS.Include(x => x.TransactionStatus)
                        .FirstOrDefaultAsync(x => x.MovementId == movementId);
                    if (existingTimeMovementToDelete == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The movement does not exist.");
                    }

                    MethodResponse responseValidateSubmission = await ValidateSubmission(existingTimeMovementToDelete, null, null,
                        null);
                    if (!responseValidateSubmission.Success)
                    {
                        return MethodResponse.CreateFailureExceptionResponse(responseValidateSubmission.Message);
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

        // CLIENT DOES NOT HAVE TRACKING TOOL - METHODS
        public async Task<MethodResponse> CreateTimeEntryTrackingTool(string userIdCreatedBy,
            CreateUpdateMovementTrackingToolVM timeEntryData)
        {
            if (timeEntryData == null)
            {
                return MethodResponse.CreateFailureExceptionResponse("Report movement data cannot be null.");
            }
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var currentUser = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userIdCreatedBy);
                    if (currentUser == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Consultant not found.");
                    }

                    if (!await _db.PROJECTS_CONSULTANTS_ASSIGNED.AnyAsync(x => x.ProjectId == timeEntryData.ProjectId && x.ConsultantId == currentUser.ConsultantId))
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The user is not assigned to the provided project.");
                    }

                    var project = await _db.PROJECTS.FirstOrDefaultAsync(x => x.ProjectId == timeEntryData.ProjectId);
                    if (project == null || project.ClientHasTrackingTool)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Invalid project configuration.");
                    }

                    MethodResponse responseValidateSubmission = await ValidateSubmission(null, timeEntryData.ActionDate, currentUser,
                        timeEntryData.ProjectId);
                    if (!responseValidateSubmission.Success)
                    {
                        return MethodResponse.CreateFailureExceptionResponse(responseValidateSubmission.Message);
                    }

                    var transactionStatus = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "No actions");
                    if (transactionStatus == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Transaction status 'No actions' not found.");
                    }
                    ReportingMyTimeMovementType? movementType = null;

                    if (timeEntryData.MovementTypeId == null)
                    {
                        movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.FirstOrDefaultAsync(x => x.Name == "Normal Hours");
                        if (movementType == null)
                        {
                            return MethodResponse.CreateFailureExceptionResponse("Movement type not valid.");
                        }
                    }
                    else
                    {
                        movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.FirstOrDefaultAsync(x => x.MovementTypeId == timeEntryData.MovementTypeId);
                        if (movementType == null)
                        {
                            return MethodResponse.CreateFailureExceptionResponse("Movement type not valid.");
                        }
                    }

                    double totalQuantity = DateAndTimes.CalculateNumHours(timeEntryData.TimeFrom, timeEntryData.TimeTo);

                    var timeMovementToCreate = new ReportingMyTimeMovement
                    {
                        ConsultantId = currentUser.ConsultantId,
                        ProjectId = (int)timeEntryData.ProjectId,
                        ActionDate = (DateTime)timeEntryData.ActionDate,
                        Notes = timeEntryData.Notes,
                        TransactionStatusId = transactionStatus.TransactionStatusId,
                        MovementTypeId = movementType.MovementTypeId,
                        CreationDate = DateTime.UtcNow,
                        TimeFrom = timeEntryData.TimeFrom,
                        TimeTo = timeEntryData.TimeTo,
                        Quantity = (decimal)totalQuantity
                    };
                    await _db.REPORTING_MY_TIME_MOVEMENTS.AddAsync(timeMovementToCreate);
                    await _db.SaveChangesAsync();
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

        public async Task<MethodResponse> AutofillTimeEntryTrackingTool(string userIdCreatedBy,
            CreateUpdateMovementTrackingToolVM timeEntryData, DateTime startDate, DateTime endDate)
        {
            if (timeEntryData == null)
            {
                return MethodResponse.CreateFailureExceptionResponse("Report movement data cannot be null.");
            }
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var currentUser = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userIdCreatedBy);
                    if (currentUser == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Consultant not found.");
                    }

                    if (!await _db.PROJECTS_CONSULTANTS_ASSIGNED.AnyAsync(x => x.ProjectId == timeEntryData.ProjectId && x.ConsultantId == currentUser.ConsultantId))
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The user is not assigned to the provided project.");
                    }

                    var project = await _db.PROJECTS.FirstOrDefaultAsync(x => x.ProjectId == timeEntryData.ProjectId);
                    if (project == null || project.ClientHasTrackingTool)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Invalid project configuration.");
                    }

                    MethodResponse responseValidateSubmission = await ValidateSubmission(null, startDate, currentUser,
                        timeEntryData.ProjectId);
                    if (!responseValidateSubmission.Success)
                    {
                        return MethodResponse.CreateFailureExceptionResponse(responseValidateSubmission.Message);
                    }

                    var transactionStatus = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == "No actions");
                    if (transactionStatus == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Transaction status 'No actions' not found.");
                    }
                    ReportingMyTimeMovementType? movementType = null;

                    if (timeEntryData.MovementTypeId == null)
                    {
                        movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.FirstOrDefaultAsync(x => x.Name == "Normal Hours");
                        if (movementType == null)
                        {
                            return MethodResponse.CreateFailureExceptionResponse("Movement type not valid.");
                        }
                    }
                    else
                    {
                        movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.FirstOrDefaultAsync(x => x.MovementTypeId == timeEntryData.MovementTypeId);
                        if (movementType == null)
                        {
                            return MethodResponse.CreateFailureExceptionResponse("Movement type not valid.");
                        }
                    }

                    //delete existing times
                    var existingTimes = await _db.REPORTING_MY_TIME_MOVEMENTS
     .Where(x => x.ConsultantId == currentUser.ConsultantId &&
                 x.ProjectId == (int)timeEntryData.ProjectId &&
                 x.ActionDate >= startDate &&
                 x.ActionDate <= endDate)
     .ToListAsync(); 

                    foreach (var movementToDelete in existingTimes)
                    {
                        _db.REPORTING_MY_TIME_MOVEMENTS.Remove(movementToDelete); 
                    }

                    double totalQuantity = DateAndTimes.CalculateNumHours(timeEntryData.TimeFrom, timeEntryData.TimeTo);

                    var startDateFormat = startDate.Date;
                    var endDateFormat = endDate.Date;

                    // Iterate over the days between TimeFrom and TimeTo
                    for (var date = startDateFormat; date <= endDateFormat; date = date.AddDays(1))
                    {
                        // Skip Saturdays and Sundays
                        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                            continue;

                        var timeMovementToCreate = new ReportingMyTimeMovement
                        {
                            ConsultantId = currentUser.ConsultantId,
                            ProjectId = (int)timeEntryData.ProjectId,
                            ActionDate = date,
                            Notes = timeEntryData.Notes,
                            TransactionStatusId = transactionStatus.TransactionStatusId,
                            MovementTypeId = movementType.MovementTypeId,
                            CreationDate = DateTime.UtcNow,
                            TimeFrom = timeEntryData.TimeFrom,
                            TimeTo = timeEntryData.TimeTo,
                            Quantity = (decimal)totalQuantity
                        };

                        await _db.REPORTING_MY_TIME_MOVEMENTS.AddAsync(timeMovementToCreate);
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return MethodResponse.CreateSuccessResponse("Autofill Completed!");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                }
            }
        }

        public async Task<MethodResponse> UpdateTimeEntryTrackingTool(string userActionedBy,
           CreateUpdateMovementTrackingToolVM timeEntryData)
        {
            var existingTimeMovement = await _db.REPORTING_MY_TIME_MOVEMENTS.FirstOrDefaultAsync(x => x.MovementId == timeEntryData.MovementId);
            if (existingTimeMovement == null)
            {
                var result = await CreateTimeEntryTrackingTool(userActionedBy, timeEntryData);
                return MethodResponse.CreateSuccessResponse("New time entry created!", result.IdCreatedElement);
            }
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var currentUser = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userActionedBy);
                    if (existingTimeMovement.ConsultantId != currentUser.ConsultantId || existingTimeMovement.ProjectId != timeEntryData.ProjectId)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The provided movement does not belong to the current user.");
                    }

                    MethodResponse responseValidateSubmission = await ValidateSubmission(null, timeEntryData.ActionDate, currentUser,
                        timeEntryData.ProjectId);
                    if (!responseValidateSubmission.Success)
                    {
                        return MethodResponse.CreateFailureExceptionResponse(responseValidateSubmission.Message);
                    }
                    ReportingMyTimeMovementType? movementType = null;
                    if (timeEntryData.MovementTypeId == null)
                    {
                        movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.FirstOrDefaultAsync(x => x.Name == "Normal Hours");
                        if (movementType == null)
                        {
                            return MethodResponse.CreateFailureExceptionResponse("Movement type not valid.");
                        }
                    }
                    else
                    {
                        movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.FirstOrDefaultAsync(x => x.MovementTypeId == timeEntryData.MovementTypeId);
                        if (movementType == null)
                        {
                            return MethodResponse.CreateFailureExceptionResponse("Movement type not valid.");
                        }
                    }

                    double totalQuantity = DateAndTimes.CalculateNumHours(timeEntryData.TimeFrom, timeEntryData.TimeTo);

                    existingTimeMovement.TimeFrom = timeEntryData.TimeFrom;
                    existingTimeMovement.TimeTo = timeEntryData.TimeTo;
                    existingTimeMovement.Quantity = (decimal)totalQuantity;
                    existingTimeMovement.Notes = timeEntryData.Notes;
                    existingTimeMovement.LastUpdateDate = DateTime.UtcNow;
                    existingTimeMovement.MovementTypeId = movementType.MovementTypeId;

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
        public async Task<List<GetTrackingToolProjectMovementsVM>> GetTrackingToolProjectMovementsAsync(int projectId, int consultId, DateTime startDate,
             DateTime endDate)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@ProjectId", projectId, DbType.Int32);
            parameters.Add("@ConsultantId", consultId, DbType.Int32);
            parameters.Add("@StartDate", startDate, DbType.Date);
            parameters.Add("@EndDate", endDate, DbType.Date);

            var results = await connection.QueryAsync<GetTrackingToolProjectMovementsVM>("SP_REPORTING_MY_TIME_GetProjectMovementsTrackingTool",
                parameters, commandType: CommandType.StoredProcedure);
            var movements = results.ToList();

            return movements;
        }
        public async Task<MethodResponse> DeleteTrackingTooTimeEntry(string userActionedBy, int movementId)
        {
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingTimeMovementToDelete = await _db.REPORTING_MY_TIME_MOVEMENTS.Include(x => x.TransactionStatus).FirstOrDefaultAsync(x => x.MovementId == movementId);
                    if (existingTimeMovementToDelete == null)
                    {
                        return MethodResponse.CreateFailureNotFoundResponse("The movement does not exist.");
                    }
                    var currentUser = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.UserId == userActionedBy);
                    if (existingTimeMovementToDelete.ConsultantId != currentUser.ConsultantId)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The provided movement does not belong to the current user.");
                    }
                    MethodResponse responseValidateSubmission = await ValidateSubmission(existingTimeMovementToDelete, null, null, null);
                    if (!responseValidateSubmission.Success)
                    {
                        return MethodResponse.CreateFailureExceptionResponse(responseValidateSubmission.Message);
                    }
                    _db.Remove(existingTimeMovementToDelete);

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return MethodResponse.CreateSuccessResponse("Time Deleted!", null);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                }
            }
        }

        // REPORTS
        public async Task<List<GlobalHoursReport>> GetGlobalMovementsWithFiltersAsync(
    DateTime startDate,
    DateTime endDate,
    int? movementTypeId,
    IEnumerable<int>? projectIds,
    IEnumerable<int>? clientIds,
    IEnumerable<int>? consultantIds)
        {
            var connection = _db.Database.GetDbConnection();

            var projectIdsTable = new DataTable();
            projectIdsTable.Columns.Add("Id", typeof(int));
            if (projectIds != null)
            {
                foreach (var id in projectIds)
                {
                    projectIdsTable.Rows.Add(id);
                }
            }

            var clientIdsTable = new DataTable();
            clientIdsTable.Columns.Add("Id", typeof(int));
            if (clientIds != null)
            {
                foreach (var id in clientIds)
                {
                    clientIdsTable.Rows.Add(id);
                }
            }

            var consultantIdsTable = new DataTable();
            consultantIdsTable.Columns.Add("Id", typeof(int));
            if (consultantIds != null)
            {
                foreach (var id in consultantIds)
                {
                    consultantIdsTable.Rows.Add(id);
                }
            }

            var parameters = new DynamicParameters();
            parameters.Add("@ProjectIds", projectIdsTable.AsTableValuedParameter("IntTableType"));
            parameters.Add("@ClientIds", clientIdsTable.AsTableValuedParameter("IntTableType"));
            parameters.Add("@ConsultantIds", consultantIdsTable.AsTableValuedParameter("IntTableType"));
            parameters.Add("@StartDate", startDate, DbType.Date);
            parameters.Add("@EndDate", endDate, DbType.Date);
            parameters.Add("@MovementTypeId", movementTypeId, DbType.Int32);

            var results = await connection.QueryAsync<GlobalHoursReport>(
                "SP_REPORTING_MY_TIME_MOVEMENTS_GetGlobalHoursReportWithFilters",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.ToList();
        }


        // SHARED STATIC METHODS
        static (DateTime StartDate, DateTime EndDate) CalculatePaymentPeriodDates(DateTime actionDate, int paymentPeriod)
        {
            DateTime startDate;
            DateTime endDate;

            if (paymentPeriod == 2)
            {
                startDate = new DateTime(actionDate.Year, actionDate.Month, 1);
                endDate = new DateTime(actionDate.Year, actionDate.Month, DateTime.DaysInMonth(actionDate.Year, actionDate.Month), 23, 59, 0);
            }
            else if (paymentPeriod == 1)
            {
                if (actionDate.Day <= 15)
                {
                    startDate = new DateTime(actionDate.Year, actionDate.Month, 1);
                    endDate = new DateTime(actionDate.Year, actionDate.Month, 15, 23, 59, 0);
                }
                else
                {
                    startDate = new DateTime(actionDate.Year, actionDate.Month, 16);
                    endDate = new DateTime(actionDate.Year, actionDate.Month, DateTime.DaysInMonth(actionDate.Year, actionDate.Month), 23, 59, 0);
                }
            }
            else
            {
                throw new ArgumentException("Invalid payment period.");
            }

            return (startDate, endDate);
        }
        public async Task<MethodResponse> ValidateSubmission(ReportingMyTimeMovement? movement, DateTime? actionDate,
            ConsultantDetail? consultant, int? projectId)
        {
            if (movement == null)
            {
                var dates = CalculatePaymentPeriodDates((DateTime)actionDate, (int)consultant.PaymentPeriod);

                var existSubmission = await _db.REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS.AnyAsync(x => x.ConsultantId == consultant.ConsultantId &&
                x.ProjectId == projectId && (x.StartPeriodDate >= dates.StartDate && x.EndPeriodDate <= dates.EndDate) &&
                x.TransactionStatus.Name != "Rejected");

                if (existSubmission)
                {
                    return MethodResponse.CreateFailureExceptionResponse("You cannot change data in a period that has already been submitted.");
                }
            }
            else
            {
                if (movement.TransactionStatus.Name != "No actions" &&
                    movement.TransactionStatus.Name != "Rejected")
                {
                    return MethodResponse.CreateFailureExceptionResponse("You cannot change data in a period that has already been submitted.");
                }
            }
            return MethodResponse.CreateSuccessResponse();
        }

        public async Task<List<GetApprovedMovementsWhereConsultantVM>> GetApprovedMovementsWhereConsultant(int consultantId,
            int projectId, DateTime startDate, DateTime endDate)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ConsultantId", consultantId);
            parameters.Add("@ProjectId", projectId);
            parameters.Add("@StartDate", startDate);
            parameters.Add("@EndDate", endDate);

            var results = await connection.QueryAsync<GetApprovedMovementsWhereConsultantVM>("SP_REPORTING_MY_TIME_MOVEMENTS_GetApprovedMovementsWhereConsultant", parameters, commandType: CommandType.StoredProcedure);
            return results.ToList();
        }
    }
}
