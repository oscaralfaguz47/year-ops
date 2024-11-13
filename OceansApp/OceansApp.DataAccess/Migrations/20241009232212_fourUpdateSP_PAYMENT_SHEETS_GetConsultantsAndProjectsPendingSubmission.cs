using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class fourUpdateSP_PAYMENT_SHEETS_GetConsultantsAndProjectsPendingSubmission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PAYMENT_SHEETS_GetConsultantsAndProjectsPendingSubmission
             @StartDate DATE,
             @EndDate DATE,
             @TransactionStatusName VARCHAR(7) = 'Pending',
             @PaymentPeriod INT
             AS
             BEGIN
                 SET NOCOUNT ON;
                 
                 WITH IsActiveStatus AS (
                 SELECT
                     PCA.ProjectConsultantAssignedId,
                     CASE
                         WHEN EXISTS (
                             SELECT 1
                             FROM (
                                 SELECT TOP 1 PCAH.IsActive, PCAH.MonthlySalary, PCAH.HourlySalary
                                 FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                 WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                   AND PCAH.ActionDate < @StartDate
                                 ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                             ) AS LastRecordBeforeStartDate
                             WHERE LastRecordBeforeStartDate.IsActive = 1
                               AND (LastRecordBeforeStartDate.MonthlySalary > 0 OR LastRecordBeforeStartDate.HourlySalary > 0)
                         ) THEN 1
                         WHEN EXISTS (
                             SELECT 1
                             FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                             WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                               AND PCAH.ActionDate BETWEEN @StartDate AND @EndDate
                               AND PCAH.IsActive = 1
                               AND (PCAH.MonthlySalary > 0 OR PCAH.HourlySalary > 0)
                         ) THEN 1
                         ELSE 0
                     END AS IsActive
                 FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
             ),
             ActiveConsultants AS (
                 SELECT 
                     PCA.ConsultantId,
                     PCA.ProjectId,
                     PCA.ProjectConsultantAssignedId
                 FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                 JOIN IsActiveStatus IAS ON PCA.ProjectConsultantAssignedId = IAS.ProjectConsultantAssignedId
                 WHERE IAS.IsActive = 1
             ),
             ActiveProjectsCount AS (
                 SELECT
                     FC.ConsultantId,
                     COUNT(DISTINCT FC.ProjectId) AS NumProjectsIsActive
                 FROM ActiveConsultants FC
                 LEFT JOIN PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS PCPDT 
                     ON FC.ProjectId = PCPDT.ProjectId
                     AND FC.ConsultantId = PCPDT.ConsultantId
                     AND PCPDT.StartPeriodDate >= @StartDate
                     AND PCPDT.EndPeriodDate <= @EndDate
                     AND PCPDT.EndPeriodDate = @EndDate
                 WHERE PCPDT.ProjectId IS NULL
                 GROUP BY FC.ConsultantId
             )
             ,PagedResults AS (
                 SELECT
                     CD.ConsultantId,
                     U.Name AS ConsultantName,
                     U.Email,
                     P.ProjectId,
                     P.Name AS ProjectName,
                     ROW_NUMBER() OVER (PARTITION BY CD.ConsultantId, P.ProjectId ORDER BY RS.SubmissionDate DESC) AS RowNum
                 FROM ActiveConsultants FC
                 JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON FC.ConsultantId = PCA.ConsultantId AND FC.ProjectId = PCA.ProjectId
                 JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                 JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                 JOIN Users U ON CD.UserId = U.Id
                 LEFT JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RS ON PCA.ConsultantId = RS.ConsultantId
                   AND P.ProjectId = RS.ProjectId AND (RS.StartPeriodDate >= @StartDate OR RS.EndPeriodDate <= @EndDate)
                   AND RS.EndPeriodDate = @EndDate
                 LEFT JOIN TRANSACTION_STATUSES TS ON RS.TransactionStatusId = TS.TransactionStatusId
                 LEFT JOIN ActiveProjectsCount APC ON FC.ConsultantId = APC.ConsultantId
                 OUTER APPLY (
                     SELECT TOP 1 PCAH.AccessToTrackingTool
                     FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                     WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                       AND PCAH.ActionDate <= @EndDate
                     ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                 ) ATTD(AccessToTrackingTool)
                 WHERE 
                     (@TransactionStatusName IS NULL OR 
                     ((@TransactionStatusName = 'Approved' AND ATTD.AccessToTrackingTool = 0 AND RS.TransactionStatusId IS NULL) 
                     OR (@TransactionStatusName = 'Pending' AND ATTD.AccessToTrackingTool <> 0 AND TS.Name IS NULL)
                     OR TS.Name = @TransactionStatusName)) 
                     AND 
                     (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
                     AND NOT EXISTS (
                         SELECT 1
                         FROM PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS PCPDT
                         WHERE PCPDT.ProjectId = P.ProjectId
                           AND PCPDT.ConsultantId = CD.ConsultantId
                           AND PCPDT.StartPeriodDate BETWEEN @StartDate AND @EndDate
                           AND PCPDT.EndPeriodDate = @EndDate
                     )
             )
             SELECT 
                 ConsultantId,
                 ConsultantName,
                 ProjectId,
                 ProjectName,
                 Email
             FROM PagedResults
             WHERE RowNum = 1;
             END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PAYMENT_SHEETS_GetConsultantsAndProjectsPendingSubmission");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_PAYMENT_SHEETS_GetConsultantsAndProjectsPendingSubmission
             @StartDate DATE,
             @EndDate DATE,
             @TransactionStatusName VARCHAR(7) = 'Pending',
             @PaymentPeriod INT
             AS
             BEGIN
                 SET NOCOUNT ON;
                 
                 WITH IsActiveStatus AS (
                 SELECT
                     PCA.ProjectConsultantAssignedId,
                     CASE
                         WHEN EXISTS (
                             SELECT 1
                             FROM (
                                 SELECT TOP 1 PCAH.IsActive, PCAH.MonthlySalary, PCAH.HourlySalary
                                 FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                 WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                   AND PCAH.ActionDate < @StartDate
                                 ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                             ) AS LastRecordBeforeStartDate
                             WHERE LastRecordBeforeStartDate.IsActive = 1
                               AND (LastRecordBeforeStartDate.MonthlySalary > 0 OR LastRecordBeforeStartDate.HourlySalary > 0)
                         ) THEN 1
                         WHEN EXISTS (
                             SELECT 1
                             FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                             WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                               AND PCAH.ActionDate BETWEEN @StartDate AND @EndDate
                               AND PCAH.IsActive = 1
                               AND (PCAH.MonthlySalary > 0 OR PCAH.HourlySalary > 0)
                         ) THEN 1
                         ELSE 0
                     END AS IsActive
                 FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
             ),
             ActiveConsultants AS (
                 SELECT 
                     PCA.ConsultantId,
                     PCA.ProjectId,
                     PCA.ProjectConsultantAssignedId
                 FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                 JOIN IsActiveStatus IAS ON PCA.ProjectConsultantAssignedId = IAS.ProjectConsultantAssignedId
                 WHERE IAS.IsActive = 1
             ),
             ActiveProjectsCount AS (
                 SELECT
                     FC.ConsultantId,
                     COUNT(DISTINCT FC.ProjectId) AS NumProjectsIsActive
                 FROM ActiveConsultants FC
                 LEFT JOIN PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS PCPDT 
                     ON FC.ProjectId = PCPDT.ProjectId
                     AND FC.ConsultantId = PCPDT.ConsultantId
                     AND PCPDT.StartPeriodDate >= @StartDate
                     AND PCPDT.EndPeriodDate <= @EndDate
                     AND PCPDT.EndPeriodDate = @EndDate
                 WHERE PCPDT.ProjectId IS NULL
                 GROUP BY FC.ConsultantId
             )
             ,PagedResults AS (
                 SELECT
                     CD.ConsultantId,
                     U.Name AS ConsultantName,
                     U.Email,
                     P.ProjectId,
                     P.Name AS ProjectName,
                     ROW_NUMBER() OVER (PARTITION BY CD.ConsultantId, P.ProjectId ORDER BY RS.SubmissionDate DESC) AS RowNum
                 FROM ActiveConsultants FC
                 JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON FC.ConsultantId = PCA.ConsultantId AND FC.ProjectId = PCA.ProjectId
                 JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                 JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                 JOIN Users U ON CD.UserId = U.Id
                 LEFT JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RS ON PCA.ConsultantId = RS.ConsultantId
                   AND P.ProjectId = RS.ProjectId AND (RS.StartPeriodDate >= @StartDate OR RS.EndPeriodDate <= @EndDate)
                   AND RS.EndPeriodDate = @EndDate
                 LEFT JOIN TRANSACTION_STATUSES TS ON RS.TransactionStatusId = TS.TransactionStatusId
                 LEFT JOIN ActiveProjectsCount APC ON FC.ConsultantId = APC.ConsultantId
                 OUTER APPLY (
                     SELECT TOP 1 PCAH.AccessToTrackingTool
                     FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                     WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                       AND PCAH.ActionDate <= @EndDate
                     ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                 ) ATTD(AccessToTrackingTool)
                 WHERE 
                     (@TransactionStatusName IS NULL OR 
                     ((@TransactionStatusName = 'Approved' AND ATTD.AccessToTrackingTool = 0 AND RS.TransactionStatusId IS NULL) 
                     OR (@TransactionStatusName = 'Pending' AND ATTD.AccessToTrackingTool <> 0 AND TS.Name IS NULL)
                     OR TS.Name = @TransactionStatusName)) 
                     AND 
                     (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
                     AND NOT EXISTS (
                         SELECT 1
                         FROM PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS PCPDT
                         WHERE PCPDT.ProjectId = P.ProjectId
                           AND PCPDT.ConsultantId = CD.ConsultantId
                           AND PCPDT.StartPeriodDate BETWEEN @StartDate AND @EndDate
                           AND PCPDT.EndPeriodDate = @EndDate
                     )
                     AND NOT EXISTS (
                         SELECT 1
                         FROM PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS PCPS
                         WHERE PCPS.ProjectId = P.ProjectId
                           AND PCPS.ConsultantId = CD.ConsultantId
                           AND PCPS.StartDate = @StartDate
                           AND PCPS.EndDate = @EndDate
                     )
             )
             SELECT 
                 ConsultantId,
                 ConsultantName,
                 ProjectId,
                 ProjectName,
                 Email
             FROM PagedResults
             WHERE RowNum = 1;
             END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_CONSULTANT_DETAILS_GetConsultantDataById");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
