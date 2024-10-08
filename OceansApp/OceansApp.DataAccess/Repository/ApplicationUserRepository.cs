using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Account;
using OceansApp.Models.ViewModels.Dashboard;
using OceansApp.Utility.ConstantData.Claims;
using SlackAPI;
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

        public async Task<List<WidgetVM>> GetWidgetsForUserAsync(ApplicationUser applicationUser, ClaimsPrincipal userClaims)
        {
            try
            {
                List<WidgetVM> listToReturn = new();

                var userCategoriesList = await _db.UserCategories.ToListAsync();

                //Access to Report time in tracking tool
                if (userClaims.IsAuthorizedForReportTimeInTrackingTool()) {
                    WidgetVM timeSheetsW = new() { WidgetName = "PendingTimeSheets" };
                    listToReturn.Add(timeSheetsW);
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
