using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.AdminCenter.ConsultantPositions;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.Consultants;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class ConsultantDetailRepository : Repository<ConsultantDetail>, IConsultantDetailRepository
    {
        private ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly UserManager<IdentityUser> _userManager;
        public ConsultantDetailRepository(ApplicationDbContext db, IConfiguration config, UserManager<IdentityUser> userManager) : base(db)
        {
            _db = db;
            _config = config;
            _userManager = userManager;
        }
        public async Task<(List<ConsultantsGetAllWithFiltersVM> consultants, int totalCount)> GetAllConsultantsWithFiltersAsync(ConsultantsPaginationFiltersVM filtersAndPagination)
        {
            var connection = _db.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", filtersAndPagination.Filters.SearchText, DbType.String);
            parameters.Add("@CountryId", filtersAndPagination.Filters.CountryId, DbType.Int32);
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

        public async Task<List<GetConsultantsBySearchTextVM>> GetConsultantsBySearchText(string searchText)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@SearchText", searchText, DbType.String);

            var result = await connection.QueryAsync<GetConsultantsBySearchTextVM>("SP_CONSULTANT_DETAILS_SearchConsultantsBySearchText", parameters, commandType: CommandType.StoredProcedure);
            return result.ToList();
        }

        public async Task<MethodResponse> CreateConsultant(string createdUserId, string userIdCreatedBy, CreateUpdateConsultantVM consultantData)
        {
            try
            {
                var timeZone = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, _config["Config:TimeZone"]);
                ConsultantDetail consultantToCreate = new()
                {
                    UserId = createdUserId,
                    CreationDate = timeZone,
                    IdCountry = consultantData.IdCountry,
                    Phone2 = consultantData.Phone2,
                    Address = consultantData.Address,
                    PersonalEmail = consultantData.PersonalEmail,
                    Location = consultantData.Location,
                    UserCreatedBy = userIdCreatedBy
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
                var timeZone = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, _config["Config:TimeZone"]);

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
                    }
                    existingUser.UserCategoryId = (int)consultantData.UserCategoryId;
                }
                existingConsultant.IdCountry = consultantData.IdCountry;
                existingConsultant.Phone2 = consultantData.Phone2;
                existingConsultant.Address = consultantData.Address;
                existingConsultant.PersonalEmail = consultantData.PersonalEmail;
                existingConsultant.Location = consultantData.Location;
                existingConsultant.LastUpdatedDate = timeZone;
                existingConsultant.UserLastUpdatedBy = userActionedBy;

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
                        Address = consultant.Address,
                        PersonalEmail = consultant.PersonalEmail,
                        Location = consultant.Location,
                        Email = consultant.Email,
                        PhoneNumber = consultant.PhoneNumber,
                        UserCategoryId = consultant.UserCategoryId,
                        UserCategoryName = consultant.UserCategoryName,
                        UserRole = consultant.UserRole,
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

    }
}
