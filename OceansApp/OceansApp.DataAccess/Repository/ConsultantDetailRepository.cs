using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.AdminCenter.ConsultantPositions;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Consultants;
using OceansApp.Models.ViewModels.PaymentSheets;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantDetailRepository : Repository<ConsultantDetail>, IConsultantDetailRepository
    {
        private ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMemoryCache _cache;
        public ConsultantDetailRepository(ApplicationDbContext db, UserManager<IdentityUser> userManager, IMemoryCache cache) : base(db)
        {
            _db = db;
            _userManager = userManager;
            _cache = cache;
        }
        public async Task<(List<ConsultantsGetAllWithFiltersVM> consultants, int totalCount)> GetAllConsultantsWithFiltersAsync(ConsultantsPaginationFiltersVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
            parameters.Add("@CountryId", filtersAndPagination.Filters.CountryId, DbType.String);
            parameters.Add("@IsTwoFactorEnabled", filtersAndPagination.Filters.IsTwoFactorEnabled, DbType.Boolean);
            parameters.Add("@EmailConfirmed", filtersAndPagination.Filters.EmailConfirmed, DbType.Boolean);
            parameters.Add("@IsActive", filtersAndPagination.Filters.IsActive, DbType.Boolean);
            parameters.Add("@UserCategoryId", filtersAndPagination.Filters.UserCategoryId, DbType.Int32);

            parameters.Add("@FieldToOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.FieldToOrder, DbType.String);
            parameters.Add("@DirectionOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.DirectionOrder, DbType.String);
            parameters.Add("@Skip", (filtersAndPagination.PaginationWithoutFilters.Pagination.PageIndex - 1) * filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@Take", filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var results = await connection.QueryAsync<ConsultantsGetAllWithFiltersVM>("SP_CONSULTANT_DETAILS_GetAllConsultantsWithFilters", parameters, commandType: CommandType.StoredProcedure);
            var totalCount = parameters.Get<int>("@TotalCount");
            var consultants = results.ToList();

            return (consultants, totalCount);
        }
        public async Task<List<GetUsersSelectVM>> GetUsersByCategoryAndPositionForSelect(string userCategory, string userPosition)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@UserCategory", userCategory, DbType.String);
            parameters.Add("@UserPosition", userPosition, DbType.String);

            var results = await connection.QueryAsync<GetUsersSelectVM>("GetUsersByCategoryAndPosition", parameters, commandType: CommandType.StoredProcedure);

            var users = results.ToList();

            return (users);
        }
        public async Task<int> GetNumOfUsersByCategoryConsultantIdAndPosition(string userCategory, string userPosition, int consultantId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserCategory", userCategory, DbType.String);
            parameters.Add("@UserPosition", userPosition, DbType.String);
            parameters.Add("@ConsultantId", consultantId, DbType.Int32);

            var result = await connection.ExecuteScalarAsync<int>("GetNumOfUsersByCategoryConsultantIdAndPosition", parameters, commandType: CommandType.StoredProcedure);
            return result;
        }
        public async Task<List<GetConsultantsBySearchTextVM>> GetConsultantsBySearchText(string? searchText, string? userCategoryName)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", searchText, DbType.String);
            parameters.Add("@UserCategoryName", userCategoryName, DbType.String);

            var result = await connection.QueryAsync<GetConsultantsBySearchTextVM>("SP_CONSULTANT_DETAILS_SearchConsultantsBySearchText", parameters, commandType: CommandType.StoredProcedure);
            return result.ToList();
        }
        public async Task<MethodResponse> CreateConsultant(string createdUserId, string userIdCreatedBy, CreateUpdateConsultantVM consultantData)
        {
            try
            {
                ConsultantDetail consultantToCreate = new()
                {
                    UserId = createdUserId,
                    CreationDate = DateTime.UtcNow,
                    IdCountry = consultantData.IdCountry,
                    Phone2 = consultantData.Phone2,
                    CompanyId = consultantData.CompanyId,
                    PaymentMethodId = consultantData.PaymentMethodId,
                    Address = consultantData.Address,
                    PersonalEmail = consultantData.PersonalEmail,
                    Location = consultantData.Location,
                    UserCreatedBy = userIdCreatedBy,
                    PaymentPeriod = consultantData.PaymentPeriod,
                    ParticipatesInOnCalls = consultantData.ParticipatesInOnCalls,
                    ConsultantHolidayId = consultantData.ConsultantHolidayId
                };
                var createdConsultant = await _db.CONSULTANT_DETAILS.AddAsync(consultantToCreate);
                await _db.SaveChangesAsync();

                if (createdConsultant.Entity.ConsultantId > 0)
                {
                    foreach (var position in consultantData.Positions)
                    {
                        ConsultantAndPosition consultantPosition = new()
                        {
                            ConsultantId = createdConsultant.Entity.ConsultantId,
                            ConsultantPositionId = position.ConsultantPositionId
                        };
                        var createdConsultantPosition = await _db.CONSULTANTS_AND_POSITIONS.AddAsync(consultantPosition);
                        await _db.SaveChangesAsync();
                        if (createdConsultant.Entity.ConsultantId < 1)
                        {
                            return new MethodResponse { MessageType = "Exception Error", Success = false, Message = "Something went wrong creating the consultant position, please try again." };
                        }
                    }
                    return new MethodResponse
                    {
                        Success = true,
                        Message = $"The Consultant {consultantData.Name} {consultantData.LastName} was created successfully.",
                        IdCreatedElement = createdConsultant.Entity.ConsultantId
                    };
                }
                else
                {
                    return new MethodResponse { MessageType = "Exception Error", Success = false, Message = "Something went wrong creating the consultant, please try again." };
                }

            }
            catch (Exception ex)
            {
                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
            }
        }
        public async Task<MethodResponse> UpdateUserConsultant(string userActionedBy, CreateUpdateConsultantVM consultantData, bool isAuthForManageAdminUsers)
        {
            try
            {
                var existingConsultant = await _db.CONSULTANT_DETAILS.FirstOrDefaultAsync(x => x.ConsultantId == consultantData.ConsultantId);
                if (existingConsultant == null)
                {
                    return new MethodResponse { MessageType = "Not Found", Success = false, Message = "The consultant was not found." };
                }
                var existingConsultantPositions = await _db.CONSULTANTS_AND_POSITIONS
                    .Where(x => x.ConsultantId == existingConsultant.ConsultantId)
                    .ToListAsync();

                if (existingConsultantPositions == null)
                {
                    return new MethodResponse { MessageType = "Not Found", Success = false, Message = "Positions were not found." };
                }
                var existingUser = await _db.AspNetUsers.FirstOrDefaultAsync(x => x.Id == existingConsultant.UserId);
                if (existingUser == null)
                {
                    return new MethodResponse { MessageType = "Not Found", Success = false, Message = "The user was not found." };
                }
                var actualUserRole = _userManager.GetRolesAsync(existingUser).Result;
                if (actualUserRole == null)
                {
                    return new MethodResponse { MessageType = "Not Found", Success = false, Message = "User role not found." };
                }

                using var transaction = await _db.Database.BeginTransactionAsync();
                _db.CONSULTANTS_AND_POSITIONS.RemoveRange(existingConsultantPositions);
                foreach (var position in consultantData.Positions)
                {
                    ConsultantAndPosition consultantPosition = new()
                    {
                        ConsultantId = existingConsultant.ConsultantId,
                        ConsultantPositionId = position.ConsultantPositionId
                    };
                    _db.CONSULTANTS_AND_POSITIONS.Add(consultantPosition);
                }
                if (isAuthForManageAdminUsers)
                {
                    if (actualUserRole[0] != consultantData.UserRole)
                    {
                        _userManager.RemoveFromRoleAsync(existingUser, actualUserRole[0]).GetAwaiter().GetResult();
                        _userManager.AddToRoleAsync(existingUser, consultantData.UserRole).GetAwaiter().GetResult();
                        var cacheKey = $"UserSessionChangesExpiration_{existingUser.Id}";
                        _cache.Set(cacheKey, DateTimeOffset.Now.AddSeconds(1), new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1)
                        });
                    }
                    existingUser.UserCategoryId = (int)consultantData.UserCategoryId;
                }
                existingConsultant.IdCountry = consultantData.IdCountry;
                existingConsultant.Phone2 = consultantData.Phone2;
                existingConsultant.CompanyId = consultantData.CompanyId;
                existingConsultant.PaymentMethodId = consultantData.PaymentMethodId;
                existingConsultant.Address = consultantData.Address;
                existingConsultant.PersonalEmail = consultantData.PersonalEmail;
                existingConsultant.Location = consultantData.Location;
                existingConsultant.LastUpdatedDate = DateTime.UtcNow;
                existingConsultant.UserLastUpdatedBy = userActionedBy;
                existingConsultant.PaymentPeriod = consultantData.PaymentPeriod;
                existingConsultant.ParticipatesInOnCalls = consultantData.ParticipatesInOnCalls;
                existingConsultant.ConsultantHolidayId = consultantData.ConsultantHolidayId;

                existingUser.Name = consultantData.Name.Trim();
                existingUser.LastName = consultantData.LastName.Trim();
                existingUser.PhoneNumber = consultantData.PhoneNumber;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new MethodResponse { Success = true, Message = $"The Consultant {consultantData.Name} {consultantData.LastName} was updated successfully." };
            }
            catch (Exception ex)
            {
                return new MethodResponse { MessageType = "Exception Error", Success = false, Message = ex.Message };
            }
        }
        public async Task<CreateUpdateConsultantVM> GetConsultantDataById(int consultantId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ConsultantId", consultantId);

            using (var multiResultSet = await connection.QueryMultipleAsync("SP_CONSULTANT_DETAILS_GetConsultantDataById", parameters, commandType: CommandType.StoredProcedure))
            {
                var consultant = await multiResultSet.ReadFirstOrDefaultAsync<CreateUpdateConsultantVM>();
                var consultantProjects = await multiResultSet.ReadAsync<CreateUpdateConsultantsAndPositionsVM>();
                if (consultant != null)
                {
                    return new CreateUpdateConsultantVM
                    {
                        ConsultantId = consultant.ConsultantId,
                        Name = consultant.Name,
                        LastName = consultant.LastName,
                        IdCountry = consultant.IdCountry,
                        CountryName = consultant.CountryName,
                        Phone2 = consultant.Phone2,
                        CompanyId = consultant.CompanyId,
                        PaymentMethodId = consultant.PaymentMethodId,
                        PaymentMethodName = consultant.PaymentMethodName,
                        Address = consultant.Address,
                        PersonalEmail = consultant.PersonalEmail,
                        Location = consultant.Location,
                        Email = consultant.Email,
                        PhoneNumber = consultant.PhoneNumber,
                        UserCategoryId = consultant.UserCategoryId,
                        UserCategoryName = consultant.UserCategoryName,
                        UserRole = consultant.UserRole,
                        PaymentPeriod = consultant.PaymentPeriod,
                        ParticipatesInOnCalls = consultant.ParticipatesInOnCalls,
                        ConsultantHolidayId = consultant.ConsultantHolidayId,
                        ConsultantHolidayName = consultant.ConsultantHolidayName,
                        Positions = (List<CreateUpdateConsultantsAndPositionsVM>)consultantProjects
                    };
                }
                else
                {
                    return null;
                }

            }
        }

        public void Update(ConsultantDetail obj)
        {
            _db.CONSULTANT_DETAILS.Update(obj);
        }

        //PAYMENT SHEETS
        public async Task<(List<PaymentSheetsGetAllWithFiltersVM> consultantsToPay, int totalCount)> GetAllConsultantsToPayWithFiltersAsync(
    PaymentSheetsPaginationFiltersVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();
            try
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                var parameters = new DynamicParameters();
                parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
                parameters.Add("@StartDate", filtersAndPagination.Filters.StartDate, DbType.Date);
                parameters.Add("@EndDate", filtersAndPagination.Filters.EndDate, DbType.Date);
                parameters.Add("@TransactionStatusName", filtersAndPagination.Filters.TransactionStatusName, DbType.String);
                parameters.Add("@ProjectId", filtersAndPagination.Filters.ProjectId, DbType.Int32);
                parameters.Add("@PaymentPeriod", filtersAndPagination.Filters.PaymentPeriod, DbType.Int32);
                parameters.Add("@FieldToOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.FieldToOrder, DbType.String);
                parameters.Add("@DirectionOrder", filtersAndPagination.PaginationWithoutFilters.OrderBy.DirectionOrder, DbType.String);
                parameters.Add("@Skip", (filtersAndPagination.PaginationWithoutFilters.Pagination.PageIndex - 1) * filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
                parameters.Add("@Take", filtersAndPagination.PaginationWithoutFilters.Pagination.PageSize, DbType.Int32);
                parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var results = await connection.QueryAsync<PaymentSheetsGetAllWithFiltersVM>("SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters", parameters, commandType: CommandType.StoredProcedure);
                var totalCount = parameters.Get<int>("@TotalCount");
                var consultantsToPay = results.ToList();
                return (consultantsToPay, totalCount);
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<GetReportDetailsFromSubmissionVM> GetReportDetailsFromSubmission(int submissionId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@SubmissionId", submissionId);

            using (var multiResultSet = await connection.QueryMultipleAsync("SP_REPORTING_MY_TIME_MOVEMENT_SUBMISSIONS_GetSubmissionReportById", parameters, commandType: CommandType.StoredProcedure))
            {
                var report = await multiResultSet.ReadFirstOrDefaultAsync<GetReportDetailsFromSubmissionVM>();
                if (report != null)
                {
                    return report;
                }
                else
                {
                    return null;
                }

            }
        }

        public async Task<MethodResponse> ApproveAndRejectSubmission(string userIdCreatedBy, ApproveRejectSubmissionVM dataFromUser)
        {
            await using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var submission = await _db.REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS.FirstOrDefaultAsync(x => x.SubmissionId == dataFromUser.SubmissionId);
                    if (submission == null)
                    {
                        return MethodResponse.CreateFailureExceptionResponse("Submission does not exist.");
                    }

                    var movements = await _db.REPORTING_MY_TIME_MOVEMENTS.Where(x => x.ProjectId == submission.ProjectId &&
                    x.ConsultantId == submission.ConsultantId && (x.ActionDate >= submission.StartPeriodDate &&
                    x.ActionDate <= submission.EndPeriodDate)).ToListAsync();


                    var transactionStatusFromDb = await _db.TRANSACTION_STATUSES.FirstOrDefaultAsync(x => x.Name == dataFromUser.TransactionStatus);
                    if (transactionStatusFromDb == null)
                    {
                        return MethodResponse.CreateFailureNotFoundResponse("Transaction status '" + dataFromUser.TransactionStatus + "' not found.");
                    }

                    foreach (var repMovement in movements)
                    {
                        repMovement.TransactionStatusId = transactionStatusFromDb.TransactionStatusId;
                    }

                    submission.TransactionStatusId = transactionStatusFromDb.TransactionStatusId;

                    if (dataFromUser.TransactionStatus == "Rejected")
                    {
                        var commentToCreate = new ReportingMyTimeComments
                        {
                            ConsultantId = submission.ConsultantId,
                            ProjectId = submission.ProjectId,
                            Body = dataFromUser.Body,
                            CreationDate = DateTime.UtcNow,
                            ActionDate = submission.EndPeriodDate,
                            UserId = userIdCreatedBy,
                            SubmissionId = submission.SubmissionId
                        };
                        await _db.REPORTING_MY_TIME_COMMENTS.AddAsync(commentToCreate);
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return MethodResponse.CreateSuccessResponse("You have " + dataFromUser.TransactionStatus + " the submission!");
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
