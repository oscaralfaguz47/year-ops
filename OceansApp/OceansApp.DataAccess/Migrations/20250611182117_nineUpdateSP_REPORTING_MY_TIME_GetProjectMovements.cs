using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class nineUpdateSP_REPORTING_MY_TIME_GetProjectMovements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_REPORTING_MY_TIME_GetProjectMovements
            @ProjectId INT,
            @ConsultantId INT,
            @StartActionDate DATE,
            @FinalActionDate DATE
            AS
            BEGIN
            SET NOCOUNT ON;
            DECLARE @TransactionStatusName NVARCHAR(450);
            DECLARE @IsDefaultProject BIT;
            DECLARE @NumHoursForHoliday INT;
            
            -- Get @TransactionStatusName
            SELECT TOP 1 
                @TransactionStatusName = TS.Name
            FROM REPORTING_MY_TIME_MOVEMENTS AS M
            INNER JOIN TRANSACTION_STATUSES AS TS 
                ON M.TransactionStatusId = TS.TransactionStatusId
            WHERE M.ProjectId = @ProjectId
              AND M.ConsultantId = @ConsultantId
              AND M.ActionDate BETWEEN @StartActionDate AND @FinalActionDate;
                    
            -- Determine @IsDefaultProject and @NumHoursForHoliday from history
            SELECT TOP 1 
                @IsDefaultProject = PCAH.IsDefaultProject,
                @NumHoursForHoliday = ISNULL(PCAH.NumHoursForHoliday, 8)
            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY AS PCAH
            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED AS PCA
                ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
            WHERE PCA.ProjectId = @ProjectId
              AND PCA.ConsultantId = @ConsultantId
              AND PCAH.ActionDate <= @FinalActionDate
            ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC;
            
            -- Use of CTEs
            WITH TimeMovements AS (
             SELECT 
                 M.MovementId,
                 MT.Name AS MovementTypeName,
                 M.Quantity,
                 M.Notes,
                 M.ActionDate,
                 TS.Name AS TransactionStatus,
                 COALESCE(
                     (
                         SELECT '[' + 
                             STRING_AGG(
                                 CAST(
                                     '{""BlobName"":""' + REPLACE(B.BlobName, '""', '""""') + '"",' +
                                     '""BlobUrl"":""' + REPLACE(B.BlobUrl, '""', '""""') + '"",' +
                                     '""PrimaryReportTrackingToolName"":' + 
                                         ISNULL('""' + REPLACE(B.PrimaryReportTrackingToolName, '""', '""""') + '""', 'null') + ',' +
                                     '""SecondReportTrackingToolName"":' + 
                                         ISNULL('""' + REPLACE(B.SecondReportTrackingToolName, '""', '""""') + '""', 'null') +
                                     '}' 
                                     AS NVARCHAR(MAX)
                                 )
                             , ','
                             ) + ']'
                         FROM REPORTING_MY_TIME_MOVEMENT_BLOBS B
                         WHERE B.MovementId = M.MovementId
                     )
                 , '[]') AS BlobData
             FROM REPORTING_MY_TIME_MOVEMENTS AS M
             INNER JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT 
                 ON M.MovementTypeId = MT.MovementTypeId
             INNER JOIN TRANSACTION_STATUSES TS 
                 ON M.TransactionStatusId = TS.TransactionStatusId
             WHERE M.ProjectId = @ProjectId
               AND M.ConsultantId = @ConsultantId
               AND M.ActionDate BETWEEN @StartActionDate AND @FinalActionDate
         ),
            HolidayDates AS (
                SELECT 
                    CHD.ConsultantHolidayDateId AS MovementId,
                    'Holidays' AS MovementTypeName,
                    @NumHoursForHoliday AS Quantity, 
                    CHD.Name AS Notes,
                    CHD.Date AS ActionDate,
                    @TransactionStatusName AS TransactionStatus,
                    '[]' AS BlobData
                FROM CONSULTANT_HOLIDAY_DATES AS CHD
                INNER JOIN CONSULTANT_HOLIDAYS AS CH 
                    ON CHD.ConsultantHolidayId = CH.ConsultantHolidayId
                INNER JOIN CONSULTANT_DETAILS AS CD 
                    ON CH.ConsultantHolidayId = CD.ConsultantHolidayId
                WHERE CD.ConsultantId = @ConsultantId
                  AND CHD.Date BETWEEN @StartActionDate AND @FinalActionDate
                  AND @IsDefaultProject = 1
            )
            SELECT * 
            FROM TimeMovements
            UNION ALL
            SELECT * 
            FROM HolidayDates
            ORDER BY MovementTypeName, ActionDate DESC;
            END;";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_REPORTING_MY_TIME_GetProjectMovements");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_REPORTING_MY_TIME_GetProjectMovements
            @ProjectId INT,
            @ConsultantId INT,
            @StartActionDate DATE,
            @FinalActionDate DATE
            AS
            BEGIN
            SET NOCOUNT ON;
            DECLARE @TransactionStatusName NVARCHAR(450);
            DECLARE @IsDefaultProject BIT;
            DECLARE @NumHoursForHoliday INT;
            
            -- Get @TransactionStatusName
            SELECT TOP 1 
                @TransactionStatusName = TS.Name
            FROM REPORTING_MY_TIME_MOVEMENTS AS M
            INNER JOIN TRANSACTION_STATUSES AS TS 
                ON M.TransactionStatusId = TS.TransactionStatusId
            WHERE M.ProjectId = @ProjectId
              AND M.ConsultantId = @ConsultantId
              AND M.ActionDate BETWEEN @StartActionDate AND @FinalActionDate;
                    
            -- Determine @IsDefaultProject and @NumHoursForHoliday from history
            SELECT TOP 1 
                @IsDefaultProject = PCAH.IsDefaultProject,
                @NumHoursForHoliday = ISNULL(PCAH.NumHoursForHoliday, 8)
            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY AS PCAH
            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED AS PCA
                ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
            WHERE PCA.ProjectId = @ProjectId
              AND PCA.ConsultantId = @ConsultantId
              AND PCAH.ActionDate <= @FinalActionDate
            ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC;
            
            -- Use of CTEs
            WITH TimeMovements AS (
                SELECT 
                    M.MovementId,
                    MT.Name AS MovementTypeName,
                    M.Quantity,
                    M.Notes,
                    M.ActionDate,
                    TS.Name AS TransactionStatus,
                   COALESCE(
                    (
                        SELECT CAST(
                            '[' + 
                            STRING_AGG(
                                '{""BlobName"":""' + REPLACE(B.BlobName, '""', '""""') + '"",' +
                                '""BlobUrl"":""' + REPLACE(B.BlobUrl, '""', '""""') + '"",' +
                                '""PrimaryReportTrackingToolName"":' + 
                                    ISNULL('""' + REPLACE(B.PrimaryReportTrackingToolName, '""', '""""') + '""', 'null') + ',' +
                                '""SecondReportTrackingToolName"":' + 
                                    ISNULL('""' + REPLACE(B.SecondReportTrackingToolName, '""', '""""') + '""', 'null') +
                                '}'
                                , ','
                            ) + ']' 
                            AS NVARCHAR(MAX)
                        )
                        FROM REPORTING_MY_TIME_MOVEMENT_BLOBS B
                        WHERE B.MovementId = M.MovementId
                    ), 
                    '[]'
                ) AS BlobData
                FROM REPORTING_MY_TIME_MOVEMENTS AS M
                INNER JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT 
                    ON M.MovementTypeId = MT.MovementTypeId
                INNER JOIN TRANSACTION_STATUSES TS 
                    ON M.TransactionStatusId = TS.TransactionStatusId
                WHERE M.ProjectId = @ProjectId
                  AND M.ConsultantId = @ConsultantId
                  AND M.ActionDate BETWEEN @StartActionDate AND @FinalActionDate
            ),
            HolidayDates AS (
                SELECT 
                    CHD.ConsultantHolidayDateId AS MovementId,
                    'Holidays' AS MovementTypeName,
                    @NumHoursForHoliday AS Quantity, 
                    CHD.Name AS Notes,
                    CHD.Date AS ActionDate,
                    @TransactionStatusName AS TransactionStatus,
                    '[]' AS BlobData
                FROM CONSULTANT_HOLIDAY_DATES AS CHD
                INNER JOIN CONSULTANT_HOLIDAYS AS CH 
                    ON CHD.ConsultantHolidayId = CH.ConsultantHolidayId
                INNER JOIN CONSULTANT_DETAILS AS CD 
                    ON CH.ConsultantHolidayId = CD.ConsultantHolidayId
                WHERE CD.ConsultantId = @ConsultantId
                  AND CHD.Date BETWEEN @StartActionDate AND @FinalActionDate
                  AND @IsDefaultProject = 1
            )
            SELECT * 
            FROM TimeMovements
            UNION ALL
            SELECT * 
            FROM HolidayDates
            ORDER BY MovementTypeName, ActionDate DESC;
            END;";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_REPORTING_MY_TIME_GetProjectMovements");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
