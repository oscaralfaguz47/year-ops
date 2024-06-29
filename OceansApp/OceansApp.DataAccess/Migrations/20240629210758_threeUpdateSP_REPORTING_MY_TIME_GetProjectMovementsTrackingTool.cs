using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class threeUpdateSP_REPORTING_MY_TIME_GetProjectMovementsTrackingTool : Migration
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
            SELECT M.MovementId,
                   M.ActionDate,
                   M.TimeFrom,
                   M.TimeTo,
                   TS.Name AS TransactionStatusName,
                   MT.IsPayable
            FROM REPORTING_MY_TIME_MOVEMENTS AS M
            INNER JOIN TRANSACTION_STATUSES AS TS 
                ON M.TransactionStatusId = TS.TransactionStatusId
            INNER JOIN REPORTING_MY_TIME_MOVEMENT_TYPES AS MT 
                ON M.MovementTypeId = MT.MovementTypeId
            WHERE M.ProjectId = @ProjectId
              AND M.ConsultantId = @ConsultantId
              AND M.ActionDate BETWEEN @StartDate AND @EndDate
            ORDER BY M.ActionDate;
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
            SELECT M.MovementId
                 , M.ActionDate
                 , M.TimeFrom
                 , M.TimeTo
                 , TS.Name AS TransactionStatusName
            FROM REPORTING_MY_TIME_MOVEMENTS M
            INNER JOIN TRANSACTION_STATUSES TS 
                ON M.TransactionStatusId = TS.TransactionStatusId
            WHERE M.ProjectId = @ProjectId
              AND M.ConsultantId = @ConsultantId
              AND M.ActionDate BETWEEN @StartDate AND @EndDate
            ORDER BY M.ActionDate;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_REPORTING_MY_TIME_GetProjectMovementsTrackingTool");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
