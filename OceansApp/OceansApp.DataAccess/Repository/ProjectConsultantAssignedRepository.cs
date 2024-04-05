using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
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

    }
}
