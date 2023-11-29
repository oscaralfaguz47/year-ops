using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.AdminCenter.UserRolesPermissions;
using System.Data;
using System.Linq.Expressions;

namespace OceansApp.DataAccess.Repository
{
    public class ApplicationSystemClaimRepository : Repository<ApplicationSystemClaim>, IApplicationSystemClaimRepository
    {
        private ApplicationDbContext _db;
        public ApplicationSystemClaimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<ApplicationSystemClaim> GetFirstOrDefaultAsync(Expression<Func<ApplicationSystemClaim, bool>> filter)
        {
            return await _db.APPLICATION_SYSTEM_CLAIMS.FirstOrDefaultAsync(filter);
        }
        public void Update(ApplicationSystemClaim obj)
        {
            _db.APPLICATION_SYSTEM_CLAIMS.Update(obj);
        }
        public IEnumerable<GetClaimsVM> GetAllPermissionsCustomData()
        {
            var permissionsList = _db.APPLICATION_SYSTEM_CLAIMS.Include(x => x.SystemSubArea)
                .OrderBy(v => v.SystemSubArea.SystemArea.Name)
                .ThenBy(v => v.SystemSubArea.Name)
                .Select(i => new GetClaimsVM
                {
                    ClaimId = i.ClaimId,
                    ClaimDescription = i.Description,
                    ClaimType = "",
                    ClaimValue = "",
                    SystemAreaName = i.SystemSubArea.SystemArea.Name,
                    SystemSubAreaName = i.SystemSubArea.Name,
                    IsAddedToTheRole = false
                }).ToList();
            return (IEnumerable<GetClaimsVM>)permissionsList;
        }
        public async Task<List<GetClaimsVM>> GetClaimsListWhereRole(string roleId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            var sqlQuery = @"
            SELECT 
                SC.ClaimId,
                SC.ClaimType,
                SC.ClaimValue,
                SC.Description AS ClaimDescription,
                SA.Name AS SystemAreaName,
                SSA.Name AS SystemSubAreaName,
                CASE WHEN EXISTS (
                SELECT 1 
                FROM RoleClaims RC 
                WHERE SC.ClaimType = RC.ClaimType AND SC.ClaimValue = RC.ClaimValue AND RC.RoleId = @roleId
                ) THEN 'true' ELSE 'false' END AS IsAddedToTheRole
                FROM 
                APPLICATION_SYSTEM_CLAIMS SC
                JOIN 
                SYSTEM_SUB_AREAS SSA ON SC.SystemSubAreaId = SSA.SystemSubAreaId
                JOIN 
                SYSTEM_AREAS SA ON SSA.SystemAreaId = SA.SystemAreaId
                ORDER BY 
                SA.Name, 
                SSA.Name;";

            parameters.Add("@roleId", roleId, DbType.String);

            var results = await connection.QueryAsync<GetClaimsVM>(sqlQuery.ToString(), parameters);

            return results.ToList();
        }

    }
}
