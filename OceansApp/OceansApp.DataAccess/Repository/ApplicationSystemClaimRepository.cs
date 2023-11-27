using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.AdminCenter.UserRolesPermissions;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class ApplicationSystemClaimRepository : Repository<ApplicationSystemClaim>, IApplicationSystemClaimRepository
    {
        private ApplicationDbContext _db;
        public ApplicationSystemClaimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(ApplicationSystemClaim obj)
        {
            _db.APPLICATION_SYSTEM_CLAIMS.Update(obj);
        }

        public async Task<List<GetClaimsVM>> GetClaimsListWhereRole(string roleId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            var sqlQuery = @"
            SELECT SC.ClaimId
            ,SC.ClaimType
            ,SC.ClaimValue
            ,SC.Description AS ClaimDescription
	        ,SA.Name AS SystemAreaName
	        ,SSA.Name AS SystemSubAreaName
            FROM APPLICATION_SYSTEM_CLAIMS SC
            JOIN RoleClaims RC ON SC.ClaimType = RC.ClaimType AND SC.ClaimValue = SC.ClaimValue
            JOIN SYSTEM_SUB_AREAS SSA ON SC.SystemSubAreaId = SSA.SystemSubAreaId
            JOIN SYSTEM_AREAS SA ON SSA.SystemAreaId = SA.SystemAreaId
            WHERE RC.RoleId = @roleId
            ORDER BY SA.Name, SSA.Name";

            parameters.Add("@roleId", roleId, DbType.String);

            var results = await connection.QueryAsync<GetClaimsVM>(sqlQuery.ToString(), parameters);

            return results.ToList();
        }

    }
}
