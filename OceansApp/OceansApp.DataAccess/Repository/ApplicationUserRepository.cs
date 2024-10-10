using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Account;
using OceansApp.Models.ViewModels.ApplicationUser;
using OceansApp.Models.ViewModels.Dashboard;
using OceansApp.Utility.ConstantData.Claims;
using System.Linq.Expressions;
using System.Security.Claims;

namespace OceansApp.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;
        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<bool> AnyAsync(Expression<Func<ApplicationUser, bool>> predicate)
        {
            return await _db.AspNetUsers.AnyAsync(predicate);
        }

        public async Task<List<GetUserIdVM>> GetUsersWhereRoleId(string roleId)
        {
            var userRoles = await _db.UserRoles.Where(x => x.RoleId == roleId).ToListAsync();
            List<GetUserIdVM> usersList = new List<GetUserIdVM>();
            foreach (var user in userRoles)
            {
                GetUserIdVM userToAdd = new GetUserIdVM()
                {
                    UserId = user.UserId
                };
                usersList.Add(userToAdd);
            }
            return usersList;
        }

        public async Task<UserAndConsultantVM> GetUserAndConsultantAsync(string userId)
        {
            var result = await (from u in _db.AspNetUsers
                                join cd in _db.CONSULTANT_DETAILS on u.Id equals cd.UserId
                                into cdGroup
                                from cd in cdGroup.DefaultIfEmpty()
                                join uc in _db.UserCategories on u.UserCategoryId equals uc.UserCategoryId
                                where u.Id == userId
                                select new UserAndConsultantVM
                                {
                                    UserId = u.Id,
                                    ConsultantId = cd.ConsultantId,
                                    Name = u.Name,
                                    LastName = u.LastName,
                                    ConsultantHolidayId = cd.ConsultantHolidayId,
                                    StartDate = cd.StartDate,
                                    WorkingModel = cd.WorkingModel,
                                    UserCategoryName = uc.Name,
                                    PaymentPeriod = (int)cd.PaymentPeriod
                                }).FirstOrDefaultAsync();

            if (result == null) throw new InvalidOperationException("The user does not exist.");

            return result;
        }
        public async Task<List<WidgetVM>> GetWidgetsForUserAsync(UserAndConsultantVM userAndConsultant, ClaimsPrincipal userClaims)
        {
            try
            {
                List<WidgetVM> listToReturn = new();

                var userCategoriesList = await _db.UserCategories.ToListAsync();

                //Access to Report time in tracking tool
                if (userClaims.IsAuthorizedForReportTimeInTrackingTool() && userAndConsultant.UserCategoryName != "External User")
                {
                    WidgetVM timeSheetsW = new() { WidgetName = "TimeSheets" };
                    listToReturn.Add(timeSheetsW);
                }
                //Access to Benefits
                if (userAndConsultant.UserCategoryName != "External User" && userAndConsultant.WorkingModel == 1)
                {
                    WidgetVM perksW = new() { WidgetName = "Perks" };
                    listToReturn.Add(perksW);
                }
                //General Consultant and Admin Team
                if (userAndConsultant.UserCategoryName != "External User")
                {
                    WidgetVM generalConsultantAndAdminW = new() { WidgetName = "GeneralConsultantAndAdmin" };
                    listToReturn.Add(generalConsultantAndAdminW);
                }

                return listToReturn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
