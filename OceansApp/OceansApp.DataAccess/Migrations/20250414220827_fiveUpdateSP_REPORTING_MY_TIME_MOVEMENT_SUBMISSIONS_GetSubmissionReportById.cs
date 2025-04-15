using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class fiveUpdateSP_REPORTING_MY_TIME_MOVEMENT_SUBMISSIONS_GetSubmissionReportById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_REPORTING_MY_TIME_MOVEMENT_SUBMISSIONS_GetSubmissionReportById
            @SubmissionId INT
            AS
            BEGIN
            -- CTE to determine the state of DefaultProject
            WITH IsDefaultProjectStatus AS (
                SELECT
                    PCA.ProjectConsultantAssignedId,
                    ISNULL(PCAH.IsDefaultProject, 0) AS IsDefaultProject,
                    ISNULL(PCAH.NumHoursForHoliday, 8) AS NumHoursForHoliday
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                INNER JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS SUB 
                    ON PCA.ProjectId = SUB.ProjectId AND PCA.ConsultantId = SUB.ConsultantId
                OUTER APPLY (
                    SELECT TOP 1 PCAH.IsDefaultProject, PCAH.NumHoursForHoliday
                    FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                    WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                      AND PCAH.ActionDate <= SUB.EndPeriodDate
                    ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                ) AS PCAH
                WHERE SUB.SubmissionId = @SubmissionId
            ),
            RegularTimeMovements AS (
                SELECT 
                    M.TimeFrom, 
                    M.TimeTo, 
                    M.Quantity, 
                    M.ActionDate, 
                    M.Notes, 
                    MT.Name AS MovementTypeName, 
                    ReportsBlobs.Blobs,
                    SUB.ProjectId,
                    SUB.ConsultantId
                FROM REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS SUB
                INNER JOIN REPORTING_MY_TIME_MOVEMENTS M ON SUB.ProjectId = M.ProjectId AND SUB.ConsultantId = M.ConsultantId
                INNER JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT ON M.MovementTypeId = MT.MovementTypeId
                OUTER APPLY (
                    SELECT B.BlobUrl, B.BlobName
                    FROM REPORTING_MY_TIME_MOVEMENT_BLOBS B
                    WHERE B.MovementId = M.MovementId
                    FOR JSON PATH
                ) AS ReportsBlobs(Blobs)
                WHERE (M.ActionDate >= CONVERT(DATE, SUB.StartPeriodDate) AND M.ActionDate <= CONVERT(DATE, SUB.EndPeriodDate))
                  AND SUB.SubmissionId = @SubmissionId
            ),
            HolidayMovements AS (
                 SELECT 
                     '08:00' AS TimeFrom,
                     FORMAT(DATEADD(MINUTE, IDPS.NumHoursForHoliday * 60, '08:00'), 'HH:mm') AS TimeTo,
                     IDPS.NumHoursForHoliday AS Quantity,
                     CHD.Date AS ActionDate,
                     CHD.Name AS Notes,
                     'Holidays' AS MovementTypeName,
                     NULL AS Blobs,
                     PCA.ProjectId,
                     PCA.ConsultantId
                 FROM CONSULTANT_HOLIDAY_DATES CHD
                 INNER JOIN CONSULTANT_HOLIDAYS CH ON CHD.ConsultantHolidayId = CH.ConsultantHolidayId
                 INNER JOIN CONSULTANT_DETAILS CD ON CH.ConsultantHolidayId = CD.ConsultantHolidayId
                 INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PCA.ConsultantId = CD.ConsultantId
                 INNER JOIN IsDefaultProjectStatus IDPS ON PCA.ProjectConsultantAssignedId = IDPS.ProjectConsultantAssignedId
                 INNER JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS SUB ON PCA.ProjectId = SUB.ProjectId AND SUB.ConsultantId = CD.ConsultantId
                 WHERE IDPS.IsDefaultProject = 1
                   AND CHD.Date >= CONVERT(DATE, SUB.StartPeriodDate)
                   AND CHD.Date <= CONVERT(DATE, SUB.EndPeriodDate)
                   AND SUB.SubmissionId = @SubmissionId
             )

            SELECT
                SUB.SubmissionDate,
                SUB.LastSubmissionDate,
                P.Name AS ProjectName,
                P.ClientHasTrackingTool,
                CONCAT(U.Name, ' ', U.LastName) AS ConsultantName,
                SUB.StartPeriodDate,
                SUB.EndPeriodDate,
                (
                    SELECT 
                        TM.TimeFrom, 
                        TM.TimeTo, 
                        TM.Quantity, 
                        TM.ActionDate, 
                        TM.Notes, 
                        TM.MovementTypeName, 
                        TM.Blobs
                    FROM (
                        SELECT * FROM RegularTimeMovements
                        UNION ALL
                        SELECT * FROM HolidayMovements
                    ) AS TM
                    WHERE TM.ProjectId = SUB.ProjectId AND TM.ConsultantId = SUB.ConsultantId
                    ORDER BY TM.ActionDate
                    FOR JSON PATH
                ) AS Movements
            FROM REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS SUB
            INNER JOIN PROJECTS P ON SUB.ProjectId = P.ProjectId
            INNER JOIN CONSULTANT_DETAILS CD ON SUB.ConsultantId = CD.ConsultantId
            INNER JOIN Users U ON CD.UserId = U.Id
            WHERE SUB.SubmissionId = @SubmissionId;
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_REPORTING_MY_TIME_MOVEMENT_SUBMISSIONS_GetSubmissionReportById");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_REPORTING_MY_TIME_MOVEMENT_SUBMISSIONS_GetSubmissionReportById
            @SubmissionId INT
            AS
            BEGIN
            -- CTE to determine the state of DefaultProject
            WITH IsDefaultProjectStatus AS (
                SELECT
                    PCA.ProjectConsultantAssignedId,
                    ISNULL(PCAH.IsDefaultProject, 0) AS IsDefaultProject,
                    ISNULL(PCAH.NumHoursForHoliday, 8) AS NumHoursForHoliday
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                INNER JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS SUB 
                    ON PCA.ProjectId = SUB.ProjectId AND PCA.ConsultantId = SUB.ConsultantId
                OUTER APPLY (
                    SELECT TOP 1 PCAH.IsDefaultProject, PCAH.NumHoursForHoliday
                    FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                    WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                      AND PCAH.ActionDate <= SUB.EndPeriodDate
                    ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                ) AS PCAH
                WHERE SUB.SubmissionId = @SubmissionId
            ),
            RegularTimeMovements AS (
                SELECT 
                    M.TimeFrom, 
                    M.TimeTo, 
                    M.Quantity, 
                    M.ActionDate, 
                    M.Notes, 
                    MT.Name AS MovementTypeName, 
                    ReportsBlobs.Blobs,
                    SUB.ProjectId,
                    SUB.ConsultantId
                FROM REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS SUB
                INNER JOIN REPORTING_MY_TIME_MOVEMENTS M ON SUB.ProjectId = M.ProjectId AND SUB.ConsultantId = M.ConsultantId
                INNER JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT ON M.MovementTypeId = MT.MovementTypeId
                OUTER APPLY (
                    SELECT B.BlobUrl, B.BlobName
                    FROM REPORTING_MY_TIME_MOVEMENT_BLOBS B
                    WHERE B.MovementId = M.MovementId
                    FOR JSON PATH
                ) AS ReportsBlobs(Blobs)
                WHERE (M.ActionDate >= CONVERT(DATE, SUB.StartPeriodDate) AND M.ActionDate <= CONVERT(DATE, SUB.EndPeriodDate))
                  AND SUB.SubmissionId = @SubmissionId
            ),
            HolidayMovements AS (
                SELECT 
                    '08:00' AS TimeFrom,
                    '16:00' AS TimeTo,
                    IDPS.NumHoursForHoliday AS Quantity,
                    CHD.Date AS ActionDate,
                    CHD.Name AS Notes,
                    'Holidays' AS MovementTypeName,
                    NULL AS Blobs,
                    PCA.ProjectId,
                    PCA.ConsultantId
                FROM CONSULTANT_HOLIDAY_DATES CHD
                INNER JOIN CONSULTANT_HOLIDAYS CH ON CHD.ConsultantHolidayId = CH.ConsultantHolidayId
                INNER JOIN CONSULTANT_DETAILS CD ON CH.ConsultantHolidayId = CD.ConsultantHolidayId
                INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PCA.ConsultantId = CD.ConsultantId
                INNER JOIN IsDefaultProjectStatus IDPS ON PCA.ProjectConsultantAssignedId = IDPS.ProjectConsultantAssignedId
                INNER JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS SUB ON PCA.ProjectId = SUB.ProjectId AND SUB.ConsultantId = CD.ConsultantId
                WHERE IDPS.IsDefaultProject = 1
                  AND CHD.Date >= CONVERT(DATE, SUB.StartPeriodDate)
                  AND CHD.Date <= CONVERT(DATE, SUB.EndPeriodDate)
                  AND SUB.SubmissionId = @SubmissionId
            )
            SELECT
                SUB.SubmissionDate,
                SUB.LastSubmissionDate,
                P.Name AS ProjectName,
                P.ClientHasTrackingTool,
                CONCAT(U.Name, ' ', U.LastName) AS ConsultantName,
                SUB.StartPeriodDate,
                SUB.EndPeriodDate,
                (
                    SELECT 
                        TM.TimeFrom, 
                        TM.TimeTo, 
                        TM.Quantity, 
                        TM.ActionDate, 
                        TM.Notes, 
                        TM.MovementTypeName, 
                        TM.Blobs
                    FROM (
                        SELECT * FROM RegularTimeMovements
                        UNION ALL
                        SELECT * FROM HolidayMovements
                    ) AS TM
                    WHERE TM.ProjectId = SUB.ProjectId AND TM.ConsultantId = SUB.ConsultantId
                    ORDER BY TM.ActionDate
                    FOR JSON PATH
                ) AS Movements
            FROM REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS SUB
            INNER JOIN PROJECTS P ON SUB.ProjectId = P.ProjectId
            INNER JOIN CONSULTANT_DETAILS CD ON SUB.ConsultantId = CD.ConsultantId
            INNER JOIN Users U ON CD.UserId = U.Id
            WHERE SUB.SubmissionId = @SubmissionId;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_REPORTING_MY_TIME_MOVEMENT_SUBMISSIONS_GetSubmissionReportById");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
