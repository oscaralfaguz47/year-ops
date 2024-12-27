using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSP_REPORTING_MY_TIME_MOVEMENTS_GetGlobalHoursReportWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE addSP_REPORTING_MY_TIME_MOVEMENTS_GetGlobalHoursReportWithFilters
             @ProjectIds IntTableType READONLY,
             @ClientIds IntTableType READONLY,
             @ConsultantIds IntTableType READONLY,
             @StartDate DATE,
             @EndDate DATE,
             @MovementTypeId INT
             AS
             BEGIN
             SET NOCOUNT ON;
             SELECT 
             CONCAT(U.Name, ' ', U.LastName) AS ConsultantName,
             PR.Name AS ProjectName,
	         CL.Name AS ClientName,
             TM.ActionDate,
             TM.TimeFrom,
             TM.TimeTo,
             TM.Quantity,
             TM.Notes,
             MT.Name AS MovementType
             FROM 
                 REPORTING_MY_TIME_MOVEMENTS TM
             INNER JOIN 
                 CONSULTANT_DETAILS CD ON TM.ConsultantId = CD.ConsultantId
             INNER JOIN 
                 Users U ON CD.UserId = U.Id
             INNER JOIN 
                 PROJECTS PR ON TM.ProjectId = PR.ProjectId
             INNER JOIN 
                 CLIENT CL ON PR.ClientId = CL.ClientId
             INNER JOIN 
                 REPORTING_MY_TIME_MOVEMENT_TYPES MT ON TM.MovementTypeId = MT.MovementTypeId
             WHERE 
                 (@MovementTypeId IS NULL OR TM.MovementTypeId = @MovementTypeId)
                 AND TM.ActionDate BETWEEN @StartDate AND @EndDate
                 AND (
                     NOT EXISTS (SELECT 1 FROM @ProjectIds) OR TM.ProjectId IN (SELECT Id FROM @ProjectIds)
                 )
                 AND (
                     NOT EXISTS (SELECT 1 FROM @ClientIds) OR PR.ClientId IN (SELECT Id FROM @ClientIds)
                 )
                 AND (
                     NOT EXISTS (SELECT 1 FROM @ConsultantIds) OR TM.ConsultantId IN (SELECT Id FROM @ConsultantIds)
                 )
             ORDER BY 
                 U.Name, PR.Name, TM.ActionDate;
             END;
             ";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS addSP_REPORTING_MY_TIME_MOVEMENTS_GetGlobalHoursReportWithFilters");
        }
    }
}
