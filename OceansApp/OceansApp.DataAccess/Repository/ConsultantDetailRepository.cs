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
        public async Task<ConsultantUserVM> GetConsultantWithUserAsync(int consultantId)
        {
            try
            {
                var result = await (from c in _db.CONSULTANT_DETAILS
                                join u in _db.AspNetUsers on c.UserId equals u.Id
                                join co in _db.COUNTRY on c.IdCountry equals co.IdCountry
                                where c.ConsultantId == consultantId
                                select new ConsultantUserVM
                                {
                                    ConsultantId = c.ConsultantId,
                                    Name = u.Name,
                                    LastName = u.LastName,
                                    Email = u.Email,
                                    PaymentMethodId = (int)c.PaymentMethodId,
                                    CompanyId = c.CompanyId,
                                    CountryName = co.Name,
                                    PaymentPeriod = (int)c.PaymentPeriod,
                                    ConsultantHolidayId = c.ConsultantHolidayId
                                }).FirstOrDefaultAsync();

            return result;
            }
            catch (Exception ex)
            {
                throw;
            }
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
                    ConsultantHolidayId = consultantData.ConsultantHolidayId,
                    WorkingModel = (int)consultantData.WorkingModel,
                    StartDate = (DateTime)consultantData.StartDate
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
                    //Create Active History
                    ApplicationUserActiveHistory activeHistoryToCreate = new()
                    {
                        ActionDate = (DateTime)consultantData.StartDate,
                        IsActive  = true,
                        UserId = createdUserId,
                        UserIdActionedBy = userIdCreatedBy
                    };
                    await _db.UsersActiveHistory.AddAsync(activeHistoryToCreate);
                    await _db.SaveChangesAsync();
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
                existingConsultant.ConsultantHolidayId = consultantData.ConsultantHolidayId;
                existingConsultant.WorkingModel = (int)consultantData.WorkingModel;
                existingConsultant.StartDate = (DateTime)consultantData.StartDate;

                existingUser.Name = consultantData.Name.Trim();
                existingUser.LastName = consultantData.LastName.Trim();
                existingUser.PhoneNumber = consultantData.PhoneNumber;

                await _db.SaveChangesAsync();

                var activeHistory = await _db.UsersActiveHistory
                                             .Where(x => x.UserId == existingConsultant.UserId && x.IsActive == true)
                                             .OrderBy(x => x.HistoryId)
                                             .ThenBy(x => x.ActionDate)
                                             .FirstOrDefaultAsync();
                if (activeHistory == null)
                {
                    //Create Active History
                    ApplicationUserActiveHistory activeHistoryToCreate = new()
                    {
                        ActionDate = (DateTime)consultantData.StartDate,
                        IsActive = true,
                        UserId = existingConsultant.UserId,
                        UserIdActionedBy = userActionedBy
                    };
                    await _db.UsersActiveHistory.AddAsync(activeHistoryToCreate);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    activeHistory.ActionDate = (DateTime)consultantData.StartDate;
                    await _db.SaveChangesAsync();
                }

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
                        ConsultantHolidayId = consultant.ConsultantHolidayId,
                        ConsultantHolidayName = consultant.ConsultantHolidayName,
                        WorkingModel = consultant.WorkingModel,
                        StartDate = consultant.StartDate,
                        Positions = (List<CreateUpdateConsultantsAndPositionsVM>)consultantProjects
                    };
                }
                else
                {
                    return null;
                }

            }
        }
        public async Task<List<GetDataForSelectVM>> GetAllConsultantsWithActiveInactiveAsync()
        {
            var consultants = await (from c in _db.CONSULTANT_DETAILS
                                join u in _db.AspNetUsers on c.UserId equals u.Id
                                select new GetDataForSelectVM
                                {
                                    Text = $"{u.Name} {u.LastName} ({(u.IsActive ? "Active" : "Inactive")})",
                                    Value = c.ConsultantId
                                }).ToListAsync();
           
            return consultants;
        }

        public async Task<List<SelectVM>> SearchConsultantsByNameAndShowInactiveAsync(string searchText, bool showInactiveConsultants)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<SelectVM>();

            try
            {
                var query = from cd in _db.CONSULTANT_DETAILS
                            join u in _db.AspNetUsers on cd.UserId equals u.Id
                            where (EF.Functions.Like(u.Name, $"%{searchText}%") ||
                                   EF.Functions.Like(u.LastName, $"%{searchText}%"))
                            select new
                            {
                                cd.ConsultantId,
                                u.Name,
                                u.LastName,
                                u.IsActive
                            };

                // Apply the active filter if needed
                if (!showInactiveConsultants)
                    query = query.Where(x => x.IsActive);

                var results = await query
                    .OrderBy(x => x.Name)
                    .Select(x => new SelectVM
                    {
                        Value = x.ConsultantId.ToString(),
                        Text = $"{x.Name} {x.LastName} - ({(x.IsActive ? "Active" : "Inactive")})"
                    })
                    .ToListAsync();

                return results;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        //PAYMENT SHEETS
        private static DataTable ToProjectIdsTvp(IEnumerable<int>? ids)
        {
            var table = new DataTable();
            table.Columns.Add("ConsultantProjectId", typeof(int));
            if (ids != null)
            {
                foreach (var id in ids.Distinct())
                    table.Rows.Add(id);
            }
            return table;
        }
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

                var projectIds = filtersAndPagination.Filters.ProjectIds ?? new List<int>();

                var tvp = ToProjectIdsTvp(projectIds);

                var parameters = new DynamicParameters();
                parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
                parameters.Add("@StartDate", filtersAndPagination.Filters.StartDate, DbType.Date);
                parameters.Add("@EndDate", filtersAndPagination.Filters.EndDate, DbType.Date);
                parameters.Add("@TransactionStatusName", filtersAndPagination.Filters.TransactionStatusName, DbType.String);
                parameters.Add("@AccountsPayableStatusName", filtersAndPagination.Filters.AccountsPayableStatusName, DbType.String);
                parameters.Add("@ProjectIds", tvp.AsTableValuedParameter("dbo.ConsultantProjectIdTableType"));
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

    }
}
