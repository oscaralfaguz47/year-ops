using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class sixUpdateSP_INTERVIEWS_GetApprovedInterviewsWhereConsultantInThePeriod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_INTERVIEWS_GetApprovedInterviewsWhereConsultantInThePeriod
            @StartDate DATE,
            @EndDate DATE,
            @ConsultantId INT
            AS
            BEGIN
            SET NOCOUNT ON;
            SELECT
                I.InterviewId AS MovementId,
                ROUND(SUM(I.DurationMinutes) / 60.0, 2) AS TotalDurationHours,
                (SELECT MovementTypeId 
                 FROM REPORTING_MY_TIME_MOVEMENT_TYPES 
                 WHERE Name = 'Interviews') AS MovementTypeId,
                I.Detail AS MovementTypeName
            FROM 
                INTERVIEWS I
            INNER JOIN 
                TRANSACTION_STATUSES TS ON I.TransactionStatusId = TS.TransactionStatusId
            WHERE 
                I.ConsultantId = @ConsultantId
                AND I.Date BETWEEN @StartDate AND @EndDate
                AND TS.Name <> 'Rejected'
            GROUP BY 
                I.InterviewId, I.Detail;
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_INTERVIEWS_GetApprovedInterviewsWhereConsultantInThePeriod");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_INTERVIEWS_GetApprovedInterviewsWhereConsultantInThePeriod
            @StartDate DATE,
            @EndDate DATE,
            @ConsultantId INT
            AS
            BEGIN
            WITH MovementType AS (
                SELECT MovementTypeId, Name
                FROM REPORTING_MY_TIME_MOVEMENT_TYPES
                WHERE Name = 'Interviews'
            )
            
            SELECT
                I.InterviewId AS MovementId,
                ROUND(SUM(I.DurationMinutes) / 60.0, 2) AS TotalDurationHours,
                MT.MovementTypeId,
                MT.Name AS MovementTypeName
            FROM INTERVIEWS I
            INNER JOIN TRANSACTION_STATUSES TS ON I.TransactionStatusId = TS.TransactionStatusId
            CROSS JOIN MovementType MT
            WHERE I.ConsultantId = @ConsultantId
              AND I.Date BETWEEN @StartDate AND @EndDate
              AND TS.Name <> 'Rejected'
            GROUP BY I.InterviewId, MT.MovementTypeId, MT.Name;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_INTERVIEWS_GetApprovedInterviewsWhereConsultantInThePeriod");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
