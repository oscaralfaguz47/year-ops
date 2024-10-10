using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.Models;
using OceansApp.Models.ViewModels.Components;
using OceansApp.Models.ViewModels.ProjecConsultantPendingSubmission;
using System.Data;

namespace OceansApp.DataAccess.Repository
{
    public class ProjectConsultantPendingSubmissionRepository : Repository<ProjectConsultantPendingSubmission>, IProjectConsultantPendingSubmissionRepository
    {
        private ApplicationDbContext _db;
        public ProjectConsultantPendingSubmissionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<List<ConsultantAndProjectVM>> GetConsultantsAndProjectsWhereSubmissionIsPendingAsync(DateTime startDate,
    DateTime endDate, int paymentPeriod)
        {
            var connection = _db.Database.GetDbConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@StartDate", startDate, DbType.Date);
            parameters.Add("@EndDate", endDate, DbType.Date);
            parameters.Add("@PaymentPeriod", paymentPeriod, DbType.Int32);

            try
            {
                await connection.OpenAsync();
                var results = await connection.QueryAsync<ConsultantAndProjectVM>(
                    "SP_PAYMENT_SHEETS_GetConsultantsAndProjectsPendingSubmission",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return results.ToList();
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<MethodResponse> CreateProjectsConsultantsPendingSubmissionsAsync(DateTime startDate,
            DateTime endDate, int paymentPeriod)
        {
            try
            {
                var consultantsAndProjectsSubmissionPending = await GetConsultantsAndProjectsWhereSubmissionIsPendingAsync(startDate,
                    endDate, paymentPeriod);

                DataTable pendingSubmissionsTable = new DataTable();
                pendingSubmissionsTable.Columns.Add("ConsultantId", typeof(int));
                pendingSubmissionsTable.Columns.Add("ProjectId", typeof(int));
                pendingSubmissionsTable.Columns.Add("StartDate", typeof(DateTime));
                pendingSubmissionsTable.Columns.Add("EndDate", typeof(DateTime));

                foreach (var submission in consultantsAndProjectsSubmissionPending)
                {
                    pendingSubmissionsTable.Rows.Add(submission.ConsultantId, submission.ProjectId, startDate, endDate);
                }

                using (SqlConnection conn = new SqlConnection(_db.Database.GetDbConnection().ConnectionString))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("SP_InsertConsultantPendingSubmissions", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlParameter parameter = cmd.Parameters.AddWithValue("@PendingSubmissions", pendingSubmissionsTable);
                        parameter.SqlDbType = SqlDbType.Structured;

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                var sentTime = await _db.PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_SENT_TIMES.FirstOrDefaultAsync(x => x.StartDate == startDate
                && x.EndDate == endDate);

                if (sentTime == null)
                {
                    ProjectConsultantPendingSubmissionSentTimes sentTimeToCreate = new()
                    {
                        StartDate = startDate,
                        EndDate = endDate,
                        NumSentTimes = 1
                    };
                    await _db.PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_SENT_TIMES.AddAsync(sentTimeToCreate);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    sentTime.NumSentTimes++;
                    await _db.SaveChangesAsync();
                }
                return MethodResponse.CreateSuccessResponseAnyList("Created successfully", consultantsAndProjectsSubmissionPending);
            }
            catch (Exception ex)
            {
                return MethodResponse.CreateFailureExceptionResponse(ex.Message);
            }
        }

        public async Task<List<ProjectsPendingSubmissionVM>> GetPendingProjectsPendingSubmissionByConsultantAsync(int consultantId,
            DateTime endDate)
        {
            var result = await (from pcps in _db.PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS
                                join p in _db.PROJECTS on pcps.ProjectId equals p.ProjectId
                                where pcps.EndDate <= endDate
                                      && pcps.ConsultantId == consultantId
                                      && !(from rmtms in _db.REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS
                                           where rmtms.ConsultantId == pcps.ConsultantId
                                                 && rmtms.ProjectId == pcps.ProjectId
                                                 && rmtms.StartPeriodDate == pcps.StartDate
                                                 && rmtms.EndPeriodDate == pcps.EndDate
                                           select rmtms).Any()
                                select new ProjectsPendingSubmissionVM
                                {
                                    ProjectId = pcps.ProjectId,
                                    ProjectName = p.Name,
                                    StartDate = pcps.StartDate,
                                    EndDate = pcps.EndDate
                                }).ToListAsync();
            return result;

        }
    }
}
