using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Account;
using System.Linq.Expressions;

namespace OceansApp.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;
        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(ApplicationUser obj)
        {
            _db.AspNetUsers.Update(obj);
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

    }
}
