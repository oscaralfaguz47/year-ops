using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.DataAccess.Services;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Blobs;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.PaymentSheets;
using OceansApp.Models.ViewModels.ReportingMyTime;
using OceansApp.Models.ViewModels.ReportingMyTime.Reports;
using OceansApp.Models.ViewModels.ReportingMyTimeMovements;
using OceansApp.Utility.SharedMethods;
using OceansApp.Utility.SharedMethods.Blobs;
using System.Data;
using System.Linq.Expressions;


namespace OceansApp.DataAccess.Repository
{
    public class ReportingMyTimeMovementRepository : Repository<ReportingMyTimeMovement>, IReportingMyTimeMovementRepository
    {
        private ApplicationDbContext _db;
        private readonly IProjectConsultantAssignedHistoryRepository _projectConsultantAssignedHistoryRepository;
        public ReportingMyTimeMovementRepository(ApplicationDbContext db, IUnitOfWork unitOfWork) : base(db)
        {
            _db = db;
            _projectConsultantAssignedHistoryRepository = unitOfWork.ProjectConsultantAssignedHistory;
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
                var currentUser = await _db.CONSULTANT_DETAILS.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userIdCreatedBy);
                if (currentUser == null)
                {
                    return MethodResponse.CreateFailureNotFoundResponse("Consultant not found.");
                }

                int? movementId = null;
                var existingMovements = await _db.REPORTING_MY_TIME_MOVEMENTS.AsNoTracking().FirstOrDefaultAsync(x => x.ActionDate >= reportMovementData.StartActionDate
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
        public async Task<MethodResponse> CreateReportingMyTimeMovementBlob(List<BlobUploadResult> uploadedBlobs, int movementId, string primarySecond, string trackingToolName)
        {
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                var errorMessage = "";
                string blobName = "";
                string? primaryTrackingTool = null;
                string? secondTrackingTool = null;

                if (primarySecond == "primary")
                {
                    primaryTrackingTool = trackingToolName;
                }
                else
                {
                    secondTrackingTool = trackingToolName;
                }
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
                                CreationDate = uploadedBlob.UploadDate,
                                PrimaryReportTrackingToolName = primaryTrackingTool,
                                SecondReportTrackingToolName = secondTrackingTool
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
        public async Task<List<IFormFile>> VerifyIfUploadFile(List<IFormFile> files, int movementId, string primarySecond, string trackingToolName)
        {
            List<IFormFile> filesToUpload = new List<IFormFile>();
            CalculateContentHash calculateHash = new CalculateContentHash();

            foreach (var file in files)
            {
                string hash = await calculateHash.CalculateContentHashAsync((IFormFile)file);
                string normalizedFileName = BlobFileNameHelper.NormalizeFileName(file.FileName);
                string fileNameWithHass = $"{hash}_{movementId}_{primarySecond}_{normalizedFileName}";

                ReportingMyTimeMovementBlob existingFile;

                if (primarySecond == "primary")
                {
                    existingFile = await _db.REPORTING_MY_TIME_MOVEMENT_BLOBS.AsNoTracking().FirstOrDefaultAsync(x => x.BlobName == fileNameWithHass
&& x.MovementId == movementId && x.PrimaryReportTrackingToolName == trackingToolName.Trim());
                }
                else
                {
                    existingFile = await _db.REPORTING_MY_TIME_MOVEMENT_BLOBS.AsNoTracking().FirstOrDefaultAsync(x => x.BlobName == fileNameWithHass
&& x.MovementId == movementId && x.SecondReportTrackingToolName == trackingToolName.Trim());
                }

                if (existingFile == null)
                {
                    filesToUpload.Add((IFormFile)file);
                }
            }
            return filesToUpload;
        }
        public async Task<int?> VerifyNumUploadedFilesPerMovementAsync(int movementId, string primarySecond, string trackingToolName)
        {
            try
            {
                if (primarySecond == "primary")
                {
                    return await _db.REPORTING_MY_TIME_MOVEMENT_BLOBS.CountAsync(x => x.MovementId == movementId && x.PrimaryReportTrackingToolName == trackingToolName);
                }
                else
                {
                    return await _db.REPORTING_MY_TIME_MOVEMENT_BLOBS.CountAsync(x => x.MovementId == movementId && x.SecondReportTrackingToolName == trackingToolName);
                }

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
                    var currentUser = await _db.CONSULTANT_DETAILS.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userIdCreatedBy);
                    if (currentUser == null)
                    {
                        return MethodResponse.CreateFailureNotFoundResponse("Consultant not found.");
                    }

                    var transactionStatusNoActions = await _db.TRANSACTION_STATUSES.AsNoTracking().FirstOrDefaultAsync(x => x.Name == "No actions");
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

                    var project = await _db.PROJECTS.AsNoTracking().FirstOrDefaultAsync(x => x.ProjectId == reportMovementData.ProjectId);
                    if (project == null || !project.ClientHasTrackingTool)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Invalid project configuration.");
                    }

                    var movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.AsNoTracking().FirstOrDefaultAsync(x => x.Name == reportMovementData.MovementType);
                    if (movementType == null)
                    {
                        return MethodResponse.CreateFailureNotFoundResponse("Movement type not valid.");
                    }
                    var existingTimeMovement = await _db.REPORTING_MY_TIME_MOVEMENTS.AsNoTracking().FirstOrDefaultAsync(x => x.ActionDate == reportMovementData.ActionDate
                    && x.MovementTypeId == movementType.MovementTypeId && x.ProjectId == reportMovementData.ProjectId && x.ConsultantId == currentUser.ConsultantId);
                    if (existingTimeMovement != null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("There is a time movement with the same action date.");
                    }

                    bool isBillable = true;
                    string? nonBillableReason = null;

                    if (!movementType.IsPayable || !project.IsBillable)
                    {
                        if (!movementType.IsPayable)
                        {
                            nonBillableReason = "This time is not paid to the consultant.";
                        }
                        if (!project.IsBillable)
                        {
                            nonBillableReason = "The project is non billable by default.";
                        }
                        isBillable = false;
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
                        IsBillable = isBillable,
                        NonBillableReason = nonBillableReason
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
                    var currentUser = await _db.CONSULTANT_DETAILS.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userActionedBy);
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
                    existingTimeMovement.UserIdLastUpdatedBy = userActionedBy;

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
                    var currentUser = await _db.CONSULTANT_DETAILS.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userIdCreatedBy);
                    if (currentUser == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Consultant not found.");
                    }

                    if (!await _db.PROJECTS_CONSULTANTS_ASSIGNED.AnyAsync(x => x.ProjectId == timeEntryData.ProjectId && x.ConsultantId == currentUser.ConsultantId))
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The user is not assigned to the provided project.");
                    }

                    var project = await _db.PROJECTS.AsNoTracking().FirstOrDefaultAsync(x => x.ProjectId == timeEntryData.ProjectId);
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

                    var transactionStatus = await _db.TRANSACTION_STATUSES.AsNoTracking().FirstOrDefaultAsync(x => x.Name == "No actions");
                    if (transactionStatus == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Transaction status 'No actions' not found.");
                    }
                    ReportingMyTimeMovementType? movementType = null;

                    if (timeEntryData.MovementTypeId == null)
                    {
                        movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.AsNoTracking().FirstOrDefaultAsync(x => x.Name == "Normal Hours");
                        if (movementType == null)
                        {
                            return MethodResponse.CreateFailureExceptionResponse("Movement type not valid.");
                        }
                    }
                    else
                    {
                        movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.AsNoTracking().FirstOrDefaultAsync(x => x.MovementTypeId == timeEntryData.MovementTypeId);
                        if (movementType == null)
                        {
                            return MethodResponse.CreateFailureExceptionResponse("Movement type not valid.");
                        }
                    }

                    double totalQuantity = DateAndTimes.CalculateNumHours(timeEntryData.TimeFrom, timeEntryData.TimeTo);

                    bool isBillable = true;
                    string nonBillableReason = timeEntryData.NonBillableReason;

                    if (!movementType.IsPayable || !project.IsBillable)
                    {
                        isBillable = false;
                        if (!movementType.IsPayable)
                        {
                            nonBillableReason = "This time is not paid to the consultant.";
                        }
                        if (!project.IsBillable)
                        {
                            nonBillableReason = "The project is non billable by default.";
                        }
                    }
                    else
                    {
                        isBillable = (bool)timeEntryData.IsBillable;
                    }

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
                        Quantity = (decimal)totalQuantity,
                        IsBillable = isBillable,
                        NonBillableReason = nonBillableReason
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
                    var currentUser = await _db.CONSULTANT_DETAILS.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userIdCreatedBy);
                    if (currentUser == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Consultant not found.");
                    }

                    if (!await _db.PROJECTS_CONSULTANTS_ASSIGNED.AnyAsync(x => x.ProjectId == timeEntryData.ProjectId && x.ConsultantId == currentUser.ConsultantId))
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The user is not assigned to the provided project.");
                    }

                    var project = await _db.PROJECTS.AsNoTracking().FirstOrDefaultAsync(x => x.ProjectId == timeEntryData.ProjectId);
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

                    var transactionStatus = await _db.TRANSACTION_STATUSES.AsNoTracking().FirstOrDefaultAsync(x => x.Name == "No actions");
                    if (transactionStatus == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Transaction status 'No actions' not found.");
                    }
                    ReportingMyTimeMovementType? movementType = null;

                    if (timeEntryData.MovementTypeId == null)
                    {
                        movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.AsNoTracking().FirstOrDefaultAsync(x => x.Name == "Normal Hours");
                        if (movementType == null)
                        {
                            return MethodResponse.CreateFailureExceptionResponse("Movement type not valid.");
                        }
                    }
                    else
                    {
                        movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.AsNoTracking().FirstOrDefaultAsync(x => x.MovementTypeId == timeEntryData.MovementTypeId);
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

                    var currentProjectHistory = await _projectConsultantAssignedHistoryRepository.GetCurrentProjectConsultantHistoryAsync(currentUser.ConsultantId, (int)timeEntryData.ProjectId, endDate);

                    IEnumerable<ConsultantHolidayDate> holidays = new List<ConsultantHolidayDate>();

                    if (currentProjectHistory.IsDefaultProject && currentProjectHistory.HolidaysMustBePaid && currentUser.ConsultantHolidayId > 0)
                    {
                        holidays = await _db.CONSULTANT_HOLIDAY_DATES
                                   .Where(x => x.ConsultantHolidayId == currentUser.ConsultantHolidayId
                                            && x.Date >= startDate
                                            && x.Date <= endDate)
                                   .ToListAsync();
                    }

                    // Spread across the period's weekdays (skipping weekends/holidays) via the shared helper.
                    var weekdayDates = WeekdaySpread.GetWeekdayDates(startDateFormat, endDateFormat,
                        holidays.Select(h => h.Date));

                    foreach (var date in weekdayDates)
                    {
                        bool isBillable = project.IsBillable;
                        string nonBillableReason = "The project is non billable by default.";

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
                            Quantity = (decimal)totalQuantity,
                            IsBillable = isBillable,
                            NonBillableReason = nonBillableReason
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

        public async Task<MethodResponse> UploadHoursOnBehalf(string actingAdminUserId, int subjectConsultantId,
            int projectId, DateTime periodStart, DateTime periodEnd, decimal hoursPerDay)
        {
            // Admin-driven autofill: AutofillTimeEntryTrackingTool + CreateSubmission fused, with the
            // actor/subject split threaded through. Movements/submission key to the SUBJECT consultant
            // (including their PaymentPeriod); the acting admin is recorded for audit. See docs/adr/0002.
            // Like autofill, hoursPerDay is the daily quantity written to EACH weekday (not a period total).
            if (string.IsNullOrWhiteSpace(actingAdminUserId))
            {
                return MethodResponse.CreateFailureExceptionResponse("Acting admin user is required.");
            }
            if (hoursPerDay <= 0)
            {
                return MethodResponse.CreateFailureValidationResponse("Enter a number of hours per day greater than zero.", "Hours");
            }

            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var subject = await _db.CONSULTANT_DETAILS.Include(x => x.ApplicationUser)
                        .FirstOrDefaultAsync(x => x.ConsultantId == subjectConsultantId);
                    if (subject == null)
                    {
                        return MethodResponse.CreateFailureNotFoundResponse("Consultant not found.");
                    }
                    if (subject.PaymentPeriod == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The consultant does not have a payment period configured.");
                    }

                    // Derive the period boundaries from the SUBJECT's PaymentPeriod so the collision guard,
                    // the weekday spread, the movement overwrite and the submission all use one canonical
                    // window (the chosen date only selects which period). See docs/adr/0002.
                    var period = CalculatePaymentPeriodDates(periodStart, subject.PaymentPeriod.Value);
                    var periodStartCanonical = period.StartDate;
                    var periodEndCanonical = period.EndDate;

                    // Requires an active assignment to the chosen project (mirror image of Feature 1).
                    if (!await _db.PROJECTS_CONSULTANTS_ASSIGNED.AnyAsync(x => x.ProjectId == projectId && x.ConsultantId == subjectConsultantId))
                    {
                        return MethodResponse.CreateFailureExceptionResponse("The consultant is not assigned to the provided project.");
                    }

                    var project = await _db.PROJECTS.AsNoTracking().FirstOrDefaultAsync(x => x.ProjectId == projectId);
                    if (project == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Invalid project configuration.");
                    }
                    // Tracking-tool projects are out of scope: autofill cannot satisfy their evidence gate. See docs/adr/0002.
                    if (project.ClientHasTrackingTool)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Manual hours upload is not available for tracking-tool projects; this case is handled manually.");
                    }

                    // Reuse the collision guard, which uses the SUBJECT's PaymentPeriod for the period math:
                    // already-submitted non-rejected period => blocked; rejected => re-submitted; drafts => overwritten.
                    MethodResponse responseValidateSubmission = await ValidateSubmission(null, periodStartCanonical, subject, projectId);
                    if (!responseValidateSubmission.Success)
                    {
                        return MethodResponse.CreateFailureExceptionResponse(responseValidateSubmission.Message);
                    }

                    var transactionStatusWaiting = await _db.TRANSACTION_STATUSES.AsNoTracking().FirstOrDefaultAsync(x => x.Name == "Waiting to be approved");
                    if (transactionStatusWaiting == null)
                    {
                        return MethodResponse.CreateFailureNotFoundResponse("Transaction status 'Waiting to be approved' not found.");
                    }
                    var movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.AsNoTracking().FirstOrDefaultAsync(x => x.Name == "Normal Hours");
                    if (movementType == null)
                    {
                        return MethodResponse.CreateFailureNotFoundResponse("Movement type 'Normal Hours' not found.");
                    }

                    var startDateFormat = periodStartCanonical.Date;
                    var endDateFormat = periodEndCanonical.Date;

                    // Resolve the subject's holidays for the period exactly as autofill does.
                    var currentProjectHistory = await _projectConsultantAssignedHistoryRepository
                        .GetCurrentProjectConsultantHistoryAsync(subject.ConsultantId, projectId, periodEndCanonical);

                    IEnumerable<ConsultantHolidayDate> holidays = new List<ConsultantHolidayDate>();
                    if (currentProjectHistory != null && currentProjectHistory.IsDefaultProject
                        && currentProjectHistory.HolidaysMustBePaid && subject.ConsultantHolidayId > 0)
                    {
                        holidays = await _db.CONSULTANT_HOLIDAY_DATES
                            .Where(x => x.ConsultantHolidayId == subject.ConsultantHolidayId
                                     && x.Date >= periodStartCanonical && x.Date <= periodEndCanonical)
                            .ToListAsync();
                    }

                    var weekdayDates = WeekdaySpread.GetWeekdayDates(startDateFormat, endDateFormat, holidays.Select(h => h.Date));
                    if (weekdayDates.Count == 0)
                    {
                        return MethodResponse.CreateFailureValidationResponse("The selected period has no weekdays to fill hours on.", "Hours");
                    }

                    // Overwrite any existing in-period movements for this consultant/project (drafts/rejected).
                    var existingTimes = await _db.REPORTING_MY_TIME_MOVEMENTS
                        .Where(x => x.ConsultantId == subject.ConsultantId && x.ProjectId == projectId
                                 && x.ActionDate >= startDateFormat && x.ActionDate <= endDateFormat)
                        .ToListAsync();
                    foreach (var movementToDelete in existingTimes)
                    {
                        _db.REPORTING_MY_TIME_MOVEMENTS.Remove(movementToDelete);
                    }

                    // Synthesize a daily TimeFrom/TimeTo window spanning hoursPerDay (e.g. 8h -> 09:00-17:00),
                    // anchored at 09:00 unless that overflows the day. Autofill movements always carry
                    // TimeFrom/TimeTo; the approvals review screen renders them, so on-behalf movements must
                    // too (a null TimeFrom breaks that screen). Quantity stays hoursPerDay, matching the window.
                    int dailyMinutes = (int)Math.Round((double)hoursPerDay * 60);
                    int startMinutes = (9 * 60 + dailyMinutes <= 24 * 60) ? 9 * 60 : 0;
                    string timeFrom = $"{startMinutes / 60:D2}:{startMinutes % 60:D2}";
                    string timeTo = $"{(startMinutes + dailyMinutes) / 60:D2}:{(startMinutes + dailyMinutes) % 60:D2}";

                    // Autofill semantics: write the same daily quantity to every weekday (skipping
                    // weekends/holidays), exactly like AutofillTimeEntryTrackingTool.
                    foreach (var date in weekdayDates)
                    {
                        var timeMovementToCreate = new ReportingMyTimeMovement
                        {
                            ConsultantId = subject.ConsultantId,
                            ProjectId = projectId,
                            ActionDate = date,
                            Notes = "Uploaded on behalf by an admin.",
                            TransactionStatusId = transactionStatusWaiting.TransactionStatusId,
                            MovementTypeId = movementType.MovementTypeId,
                            CreationDate = DateTime.UtcNow,
                            TimeFrom = timeFrom,
                            TimeTo = timeTo,
                            Quantity = hoursPerDay,
                            IsBillable = project.IsBillable,
                            NonBillableReason = "The project is non billable by default.",
                            UserIdLastUpdatedBy = actingAdminUserId  // actor stamped on each movement
                        };
                        await _db.REPORTING_MY_TIME_MOVEMENTS.AddAsync(timeMovementToCreate);
                    }

                    // Create or re-submit the submission at "Waiting to be approved" — NEVER auto-approved.
                    var existingSubmission = await _db.REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS
                        .Include(x => x.TransactionStatus)
                        .FirstOrDefaultAsync(x => x.ConsultantId == subject.ConsultantId && x.ProjectId == projectId
                            && x.StartPeriodDate.Date == startDateFormat && x.EndPeriodDate.Date == endDateFormat);

                    ReportingMyTimeMovementSubmission submission;
                    if (existingSubmission == null)
                    {
                        submission = new ReportingMyTimeMovementSubmission
                        {
                            ConsultantId = subject.ConsultantId,
                            ProjectId = projectId,
                            TransactionStatusId = transactionStatusWaiting.TransactionStatusId,
                            SubmissionDate = DateTime.UtcNow,
                            StartPeriodDate = periodStartCanonical,
                            EndPeriodDate = periodEndCanonical
                        };
                        await _db.REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS.AddAsync(submission);
                    }
                    else
                    {
                        // ValidateSubmission only let us through for a rejected (or absent) submission.
                        existingSubmission.LastSubmissionDate = DateTime.UtcNow;
                        existingSubmission.TransactionStatusId = transactionStatusWaiting.TransactionStatusId;
                        submission = existingSubmission;
                    }

                    await _db.SaveChangesAsync();

                    // Audit trail: one "uploaded on behalf of {consultant} by {admin}" comment on the submission.
                    var subjectName = $"{subject.ApplicationUser?.Name} {subject.ApplicationUser?.LastName}".Trim();
                    var admin = await _db.AspNetUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == actingAdminUserId);
                    var adminName = admin == null ? actingAdminUserId : $"{admin.Name} {admin.LastName}".Trim();

                    var comment = new ReportingMyTimeComments
                    {
                        ConsultantId = subject.ConsultantId,
                        ProjectId = projectId,
                        Body = $"Uploaded on behalf of {subjectName} by {adminName}.",
                        CreationDate = DateTime.UtcNow,
                        ActionDate = periodEndCanonical,
                        UserId = actingAdminUserId,
                        SubmissionId = submission.SubmissionId
                    };
                    await _db.REPORTING_MY_TIME_COMMENTS.AddAsync(comment);

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return MethodResponse.CreateSuccessResponse(
                        "Hours uploaded on behalf of the consultant. The submission is waiting to be approved.",
                        submission.SubmissionId);
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

            var project = await _db.PROJECTS.AsNoTracking().FirstOrDefaultAsync(x => x.ProjectId == existingTimeMovement.ProjectId);
            if (project == null || project.ClientHasTrackingTool)
            {
                return MethodResponse.CreateFailureExceptionResponse("Invalid project configuration.");
            }
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var currentUser = await _db.CONSULTANT_DETAILS.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userActionedBy);
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
                        movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.AsNoTracking().FirstOrDefaultAsync(x => x.Name == "Normal Hours");
                        if (movementType == null)
                        {
                            return MethodResponse.CreateFailureExceptionResponse("Movement type not valid.");
                        }
                    }
                    else
                    {
                        movementType = await _db.REPORTING_MY_TIME_MOVEMENT_TYPES.AsNoTracking().FirstOrDefaultAsync(x => x.MovementTypeId == timeEntryData.MovementTypeId);
                        if (movementType == null)
                        {
                            return MethodResponse.CreateFailureExceptionResponse("Movement type not valid.");
                        }
                    }

                    double totalQuantity = DateAndTimes.CalculateNumHours(timeEntryData.TimeFrom, timeEntryData.TimeTo);

                    bool isBillable = true;
                    string nonBillableReason = timeEntryData.NonBillableReason;

                    if (!movementType.IsPayable || !project.IsBillable)
                    {
                        isBillable = false;
                        if (!movementType.IsPayable)
                        {
                            nonBillableReason = "This time is not paid to the consultant.";
                        }
                        if (!project.IsBillable)
                        {
                            nonBillableReason = "The project is non billable by default.";
                        }
                    }
                    else
                    {
                        isBillable = (bool)timeEntryData.IsBillable;
                    }

                    existingTimeMovement.TimeFrom = timeEntryData.TimeFrom;
                    existingTimeMovement.TimeTo = timeEntryData.TimeTo;
                    existingTimeMovement.Quantity = (decimal)totalQuantity;
                    existingTimeMovement.Notes = timeEntryData.Notes;
                    existingTimeMovement.LastUpdateDate = DateTime.UtcNow;
                    existingTimeMovement.UserIdLastUpdatedBy = userActionedBy;
                    existingTimeMovement.MovementTypeId = movementType.MovementTypeId;
                    existingTimeMovement.IsBillable = isBillable;
                    existingTimeMovement.NonBillableReason = nonBillableReason;

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
                    var currentUser = await _db.CONSULTANT_DETAILS.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userActionedBy);
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

        public async Task<List<GetBillableHoursForBillingVM>> GetBillableHoursForBillingAsync(int clientId, DateTime startDate,
            DateTime endDate)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@ClientId", clientId, DbType.Int32);
            parameters.Add("@StartDate", startDate, DbType.Date);
            parameters.Add("@EndDate", endDate, DbType.Date);

            var results = await connection.QueryAsync<GetBillableHoursForBillingVM>("SP_REPORTING_MY_TIME_MOVEMENTS_GetBillableHoursByClient",
                parameters, commandType: CommandType.StoredProcedure);
            var movements = results.ToList();

            return movements;
        }

        // PAYMENT SHEETS
        public async Task<MethodResponse> UpdateTimeFromPaymentSheets(string userActionedBy,
          List<EditHoursFromPaymentSheetsVM> timeList)
        {
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                foreach (var item in timeList)
                {
                    var existingTimeMovement = await _db.REPORTING_MY_TIME_MOVEMENTS.Include(x => x.ReportingMyTimeMovementType)
                        .FirstOrDefaultAsync(x => x.MovementId == item.MovementId);

                    if (existingTimeMovement == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse($"The movementId: {item.MovementId} does not exist.");
                    }

                    try
                    {
                        if ((bool)item.Remove && existingTimeMovement.TimeFrom != null && existingTimeMovement.TimeTo != null)
                        {
                            _db.REPORTING_MY_TIME_MOVEMENTS.Remove(existingTimeMovement);
                        }
                        else
                        {
                            if (existingTimeMovement.TimeFrom == null && existingTimeMovement.TimeTo == null)
                            {
                                if (existingTimeMovement.ReportingMyTimeMovementType.Name != "Normal Hours" && item.Quantity <= 0)
                                {
                                    _db.REPORTING_MY_TIME_MOVEMENTS.Remove(existingTimeMovement);
                                }
                                if (existingTimeMovement.ReportingMyTimeMovementType.Name == "Normal Hours" && item.Quantity <= 0)
                                {
                                    return MethodResponse.CreateFailureValidationResponse("The Quantity of the Normal Hours must be greater than zero.");
                                }

                                if (existingTimeMovement.Quantity != item.Quantity && item.Quantity > 0)
                                {
                                    existingTimeMovement.Quantity = (decimal)item.Quantity;
                                    existingTimeMovement.LastUpdateDate = DateTime.UtcNow;
                                    existingTimeMovement.UserIdLastUpdatedBy = userActionedBy;
                                }
                            }
                            else
                            {
                                if (existingTimeMovement.TimeFrom != item.TimeFrom || existingTimeMovement.TimeTo != item.TimeTo)
                                {
                                    double totalQuantity = DateAndTimes.CalculateNumHours(item.TimeFrom, item.TimeTo);

                                    existingTimeMovement.TimeFrom = item.TimeFrom;
                                    existingTimeMovement.TimeTo = item.TimeTo;
                                    existingTimeMovement.Quantity = (decimal)totalQuantity;
                                    existingTimeMovement.LastUpdateDate = DateTime.UtcNow;
                                    existingTimeMovement.UserIdLastUpdatedBy = userActionedBy;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return MethodResponse.CreateFailureExceptionResponse(ex.Message);
                    }
                }
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return MethodResponse.CreateSuccessResponse("Changes saved!");
            }
        }
    }
}
