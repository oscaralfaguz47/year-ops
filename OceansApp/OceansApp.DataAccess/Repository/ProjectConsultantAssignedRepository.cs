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

        public async Task<GetConsultantStatusInTheProjectVM> GetConsultantStatusInTheProject(string userId, DateTime startDate,
            DateTime endDate)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId, DbType.String);
            parameters.Add("@StartDate", startDate);
            parameters.Add("@EndDate", endDate);

            var info = await connection.QuerySingleOrDefaultAsync<GetConsultantStatusInTheProjectVM>("SP_PROJECTS_CONSULTANT_ASSIGNED_GetConsultantStatusInTheProject",
                parameters, commandType: CommandType.StoredProcedure);
            return info;
        }

        public async Task<List<ProjectActiveAndAccessTrackingTool>> GetProjectsWhereActiveAndAccessToTrackingTool(int consultantId)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ConsultantId", consultantId, DbType.String);

            var activeProjectsAccessTrackingTool = await connection.QuerySingleOrDefaultAsync<List<ProjectActiveAndAccessTrackingTool>>("SP_PROJECTS_CONSULTANT_ASSIGNED_GetProjectsWhereActiveAndAccessToTrackingTool",
                parameters, commandType: CommandType.StoredProcedure);

            return activeProjectsAccessTrackingTool;
        }

        //PAYMENT SHEETS
        public async Task<List<GetProjectInfoWhereConsultantIsActiveInProjectVM>> GetProjectsInfoWhereConsultantIsActiveInPeriod(int consultantId, DateTime startDate,
            DateTime endDate)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ConsultantId", consultantId);
            parameters.Add("@StartDate", startDate);
            parameters.Add("@EndDate", endDate);

            var results = await connection.QueryAsync<GetProjectInfoWhereConsultantIsActiveInProjectVM>("SP_PAYMENT_SHEETS_GetProjectsInfoWhereConsultantIsActiveInPeriod", parameters, commandType: CommandType.StoredProcedure);
            return results.ToList();
        }

    }
}
