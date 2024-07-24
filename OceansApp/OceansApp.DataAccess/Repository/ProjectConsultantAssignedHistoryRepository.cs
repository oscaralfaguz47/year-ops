using Dapper;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Projects;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class ProjectConsultantAssignedHistoryRepository : Repository<ProjectConsultantAssignedHistory>, IProjectConsultantAssignedHistoryRepository
    {
        private ApplicationDbContext _db;
        public ProjectConsultantAssignedHistoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<List<GetProjectConsultantAssignedHistoryVM>> GetProjectConsultantAssignedHistoryByAssignationId(int projectConsultantAssignedId, string? userCategoryName)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ProjectConsultantAssignedId", projectConsultantAssignedId, DbType.Int32);
            parameters.Add("@UserCategoryName", userCategoryName, DbType.String);
            var results = await connection.QueryAsync<GetProjectConsultantAssignedHistoryVM>("SP_PROJECT_GetProjectConsultantAssignedHistoryByAssignationId", parameters, commandType: CommandType.StoredProcedure);
            return results.ToList();
        }

    }
}
