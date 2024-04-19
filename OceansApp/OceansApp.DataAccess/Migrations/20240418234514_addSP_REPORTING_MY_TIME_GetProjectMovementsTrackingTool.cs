using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSP_REPORTING_MY_TIME_GetProjectMovementsTrackingTool : Migration
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
            SELECT M.MovementId
            ,M.ActionDate
            ,M.TimeFrom
            ,M.TimeTo
            ,M.Notes
            ,TS.Name AS TransactionStatusName
            FROM REPORTING_MY_TIME_MOVEMENTS M
            INNER JOIN TRANSACTION_STATUSES TS ON M.TransactionStatusId = TS.TransactionStatusId
            WHERE M.ProjectId = @ProjectId
            AND M.ConsultantId = @ConsultantId
            AND (M.ActionDate >= @StartDate AND M.ActionDate <= @EndDate)
            ORDER BY M.ActionDate;
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_REPORTING_MY_TIME_GetProjectMovementsTrackingTool");
        }
    }
}
