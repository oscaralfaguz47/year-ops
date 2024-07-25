using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class eightUpdateSP_REPORTING_MY_TIME_GetProjectMovementsTrackingTool : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_REPORTING_MY_TIME_GetProjectMovementsTrackingTool
            @ProjectId INT,
            @ConsultantId INT,
            @StartDate DATE,
            @EndDate DATE
            AS
            BEGIN
            DECLARE @TransactionStatusName NVARCHAR(450);
            DECLARE @IsDefaultProject BIT;
            
            -- Get @TransactionStatusName
            SELECT TOP 1 
                @TransactionStatusName = TS.Name
            FROM REPORTING_MY_TIME_MOVEMENTS AS M
            INNER JOIN TRANSACTION_STATUSES AS TS 
                ON M.TransactionStatusId = TS.TransactionStatusId
            WHERE M.ProjectId = @ProjectId
              AND M.ConsultantId = @ConsultantId
              AND M.ActionDate BETWEEN @StartDate AND @EndDate;
            
            -- Determine @IsDefaultProject from PROJECTS_CONSULTANTS_ASSIGNED_HISTORY
            SELECT TOP 1 @IsDefaultProject = PCAH.IsDefaultProject
            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY AS PCAH
            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED AS PCA
                ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
            WHERE PCA.ProjectId = @ProjectId
              AND PCA.ConsultantId = @ConsultantId
              AND PCAH.ActionDate <= @EndDate
            ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC;
            
            -- Use of CTEs
            WITH TimeMovements AS (
                SELECT 
                    M.MovementId,
                    M.ActionDate,
                    M.TimeFrom,
                    M.TimeTo,
                    TS.Name AS TransactionStatusName,
                    MT.IsPayable,
                    M.Notes
                FROM REPORTING_MY_TIME_MOVEMENTS AS M
                INNER JOIN TRANSACTION_STATUSES AS TS 
                    ON M.TransactionStatusId = TS.TransactionStatusId
                INNER JOIN REPORTING_MY_TIME_MOVEMENT_TYPES AS MT 
                    ON M.MovementTypeId = MT.MovementTypeId
                WHERE M.ProjectId = @ProjectId
                  AND M.ConsultantId = @ConsultantId
                  AND M.ActionDate BETWEEN @StartDate AND @EndDate
            ),
            HolidayDates AS (
                SELECT 
                    CHD.ConsultantHolidayDateId AS MovementId,
                    CHD.Date AS ActionDate,
                    'Holiday' AS TimeFrom,
                    NULL AS TimeTo,
                    @TransactionStatusName AS TransactionStatusName,
                    1 AS IsPayable,
                    CHD.Name AS Notes
                FROM CONSULTANT_HOLIDAY_DATES AS CHD
                INNER JOIN CONSULTANT_HOLIDAYS AS CH 
                    ON CHD.ConsultantHolidayId = CH.ConsultantHolidayId
                INNER JOIN CONSULTANT_DETAILS AS CD 
                    ON CH.ConsultantHolidayId = CD.ConsultantHolidayId
                WHERE CD.ConsultantId = @ConsultantId
                  AND CHD.Date BETWEEN @StartDate AND @EndDate
                  AND @IsDefaultProject = 1
            )
            SELECT * 
            FROM TimeMovements
            UNION ALL
            SELECT * 
            FROM HolidayDates
            ORDER BY ActionDate;
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_REPORTING_MY_TIME_GetProjectMovementsTrackingTool");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_REPORTING_MY_TIME_GetProjectMovementsTrackingTool
            @ProjectId INT,
            @ConsultantId INT,
            @StartDate DATE,
            @EndDate DATE
            AS
            BEGIN
            DECLARE @TransactionStatusName NVARCHAR(450);

            SELECT TOP 1 @TransactionStatusName = TS.Name
            FROM REPORTING_MY_TIME_MOVEMENTS AS M
            INNER JOIN TRANSACTION_STATUSES AS TS 
                ON M.TransactionStatusId = TS.TransactionStatusId
            WHERE M.ProjectId = @ProjectId
              AND M.ConsultantId = @ConsultantId
              AND M.ActionDate BETWEEN @StartDate AND @EndDate;
            
            DECLARE @IsDefaultProject BIT;
            
            SELECT @IsDefaultProject = PCA.IsDefaultProject
            FROM PROJECTS_CONSULTANTS_ASSIGNED AS PCA
            WHERE PCA.ProjectId = @ProjectId
              AND PCA.ConsultantId = @ConsultantId;
            
            WITH TimeMovements AS (
                SELECT M.MovementId,
                       M.ActionDate,
                       M.TimeFrom,
                       M.TimeTo,
                       TS.Name AS TransactionStatusName,
                       MT.IsPayable,
                       M.Notes
                FROM REPORTING_MY_TIME_MOVEMENTS AS M
                INNER JOIN TRANSACTION_STATUSES AS TS 
                    ON M.TransactionStatusId = TS.TransactionStatusId
                INNER JOIN REPORTING_MY_TIME_MOVEMENT_TYPES AS MT 
                    ON M.MovementTypeId = MT.MovementTypeId
                WHERE M.ProjectId = @ProjectId
                  AND M.ConsultantId = @ConsultantId
                  AND M.ActionDate BETWEEN @StartDate AND @EndDate
            ),
            
            HolidayDates AS (
                SELECT 
                    CHD.ConsultantHolidayDateId AS MovementId,
                    CHD.Date AS ActionDate,
                    'Holiday' AS TimeFrom,
                    NULL AS TimeTo,
                    @TransactionStatusName AS TransactionStatusName,
                    1 AS IsPayable,
                    CHD.Name AS Notes
                FROM CONSULTANT_HOLIDAY_DATES AS CHD
                INNER JOIN CONSULTANT_HOLIDAYS AS CH 
                    ON CHD.ConsultantHolidayId = CH.ConsultantHolidayId
                INNER JOIN CONSULTANT_DETAILS AS CD 
                    ON CH.ConsultantHolidayId = CD.ConsultantHolidayId
                WHERE CD.ConsultantId = @ConsultantId
                  AND CHD.Date BETWEEN @StartDate AND @EndDate
                  AND @IsDefaultProject = 1
            )
            
            SELECT * FROM TimeMovements
            UNION ALL
            SELECT * FROM HolidayDates
            ORDER BY ActionDate;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_REPORTING_MY_TIME_GetProjectMovementsTrackingTool");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
