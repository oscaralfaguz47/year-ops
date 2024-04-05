using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.ProjectConsultantAssigned;
using OceansApp.Models.ViewModels.Projects;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class ProjectConsultantAssignedRepository : Repository<ProjectConsultantAssigned>, IProjectConsultantAssignedRepository
    {
        private ApplicationDbContext _db;
        public ProjectConsultantAssignedRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<List<GetProjectsListVM>> GetProjectsWhereConsultantAssigned(string? userId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId, DbType.String);
            var results = await connection.QueryAsync<GetProjectsListVM>("SP_PROJECTS_CONSULTANTS_ASSIGNED_GetProjectsWhereConsultantAssigned", parameters, commandType: CommandType.StoredProcedure);
            return results.ToList();
        }

        public async Task<GetConsultantSelectedProjectInfoVM> GetConsultantSelectedProjectInfo(string userId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId, DbType.String);

            var info = await connection.QuerySingleOrDefaultAsync<GetConsultantSelectedProjectInfoVM>("SP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo",
                parameters, commandType: CommandType.StoredProcedure);
            return info;
        }

    }
}
