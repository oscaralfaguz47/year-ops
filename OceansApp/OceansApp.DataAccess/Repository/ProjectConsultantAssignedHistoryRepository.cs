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
        public async Task<ProjectConsultantAssignedHistory> GetCurrentProjectConsultantHistoryAsync(int consultantId, int projectId,
            DateTime endDate)
        {
           var currentHistory = await (from pca in _db.PROJECTS_CONSULTANTS_ASSIGNED
                                       join h in _db.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY on pca.ProjectConsultantAssignedId 
                                       equals h.ProjectConsultantAssignedId
                                       where pca.ProjectId == projectId
                                       && pca.ConsultantId == consultantId
                                       && h.ActionDate <= endDate.Date
                                       orderby h.ActionDate descending, h.Id descending
                                       select new ProjectConsultantAssignedHistory
                                       {
                                           ProjectConsultantAssignedId = h.ProjectConsultantAssignedId,
                                           ActionDate = h.ActionDate,
                                           PositionId = h.PositionId,
                                           MonthlySalary = h.MonthlySalary,
                                           MonthlySalaryPartner = h.MonthlySalaryPartner,
                                           Id = h.Id,
                                           CreationDate = h.CreationDate,
                                           AccessToTrackingTool = h.AccessToTrackingTool,
                                           HolidaysMustBePaid = h.HolidaysMustBePaid,
                                           HourlyClientRate  = h.HourlyClientRate,
                                           HourlySalary = h.HourlySalary,
                                           IsActive = h.IsActive,
                                           IsDefaultProject = h.IsDefaultProject,
                                           IsMonthlySalaryCalculatedPerHour = h.IsMonthlySalaryCalculatedPerHour,
                                           MonthlyClientRate = h.MonthlyClientRate,
                                           PartnerId = h.PartnerId,
                                           PartnerPaysBenefits = h.PartnerPaysBenefits,
                                           UserIdActionedBy = h.UserIdActionedBy,
                                           ParticipatesInOnCalls = h.ParticipatesInOnCalls
                                       }).FirstOrDefaultAsync(); 
            return currentHistory;
        }

    }
}
