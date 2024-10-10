using Dapper;
using Microsoft.ApplicationInsights;
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
        private readonly TelemetryClient _telemetryClient;
        public ProjectConsultantPendingSubmissionRepository(ApplicationDbContext db, TelemetryClient telemetryClient) : base(db)
        {
            _db = db;
            _telemetryClient = telemetryClient;
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

                // Usa la conexión existente en lugar de abrir una nueva
                var connection = (SqlConnection)_db.Database.GetDbConnection();

                // Abre la conexión solo si está cerrada
                if (connection.State == ConnectionState.Closed)
                {
                    await connection.OpenAsync();
                }

                using (SqlCommand cmd = new SqlCommand("SP_InsertConsultantPendingSubmissions", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter parameter = cmd.Parameters.AddWithValue("@PendingSubmissions", pendingSubmissionsTable);
                    parameter.SqlDbType = SqlDbType.Structured;

                    await cmd.ExecuteNonQueryAsync();
                }

                // Continúa con la lógica de verificación y actualización de `sentTime`
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
                _telemetryClient.TrackException(ex);
                Console.WriteLine(ex.ToString());
                return MethodResponse.CreateFailureExceptionResponse(ex.Message);
            }
            finally
            {
                // Cierra la conexión si está abierta
                if (_db.Database.GetDbConnection().State == ConnectionState.Open)
                {
                    await _db.Database.GetDbConnection().CloseAsync();
                }
            }
        }


        public async Task<List<ProjectsPendingSubmissionVM>> GetPendingProjectsPendingSubmissionByConsultantAsync(int consultantId,
            DateTime endDate)
        {
            var result = await (from pcps in _db.PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS.AsNoTracking()
                                join p in _db.PROJECTS.AsNoTracking() on pcps.ProjectId equals p.ProjectId
                                join rmtms in _db.REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS.AsNoTracking()
                                    on new { pcps.ProjectId, pcps.ConsultantId, pcps.StartDate, pcps.EndDate }
                                    equals new { rmtms.ProjectId, rmtms.ConsultantId, StartDate = rmtms.StartPeriodDate, EndDate = rmtms.EndPeriodDate }
                                    into rmGroup
                                from rmtms in rmGroup.DefaultIfEmpty()
                                join pct in _db.PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS.AsNoTracking()
                                    on new { pcps.ProjectId, pcps.ConsultantId, pcps.StartDate, pcps.EndDate }
                                    equals new { pct.ProjectId, pct.ConsultantId, StartDate = pct.StartPeriodDate, EndDate = pct.EndPeriodDate }
                                    into pctGroup
                                from pct in pctGroup.DefaultIfEmpty()
                                where pcps.EndDate <= endDate
                                      && pcps.ConsultantId == consultantId
                                      && rmtms == null
                                      && pct == null 
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
