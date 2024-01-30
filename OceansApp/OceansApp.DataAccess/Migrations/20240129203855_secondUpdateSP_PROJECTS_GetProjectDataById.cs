using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class secondUpdateSP_PROJECTS_GetProjectDataById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PROJECTS_GetProjectDataById
            @ProjectId INT
            AS
            BEGIN
            SELECT 
                ProjectId
                ,P.Name
                ,P.Description
                ,P.StartDate
                ,P.IsActive
                ,P.IsBillable
                ,P.ClientId
                ,C.Name
                ,P.SuccessManagerId
                ,U.Name + ' ' + U.LastName AS SuccessManagerName
                ,P.ClientHasTrackingTool
                FROM PROJECTS P
                JOIN CONSULTANT_DETAILS CD ON P.SuccessManagerId = CD.ConsultantId
                JOIN Users U ON CD.UserId = U.Id
                JOIN CLIENT C ON P.ClientId = C.ClientId
                WHERE ProjectId = @ProjectId

            SELECT
                ProjectConsultantAssignedId
                ,ConsultantId
                ,HourlyClientRate
                ,HourlySalary
                ,MonthlyClientRate
                ,MonthlySalary
                ,PositionDetail
                FROM PROJECTS_CONSULTANTS_ASSIGNED WHERE ProjectId = @ProjectId
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECTS_GetProjectDataById");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_PROJECTS_GetProjectDataById
            @ProjectId INT
            AS
            BEGIN
            SELECT 
                ProjectId
                ,P.Name
                ,P.Description
                ,P.StartDate
                ,P.IsActive
                ,P.IsBillable
                ,P.ClientId
                ,P.SuccessManagerId
                ,U.Name + ' ' + U.LastName AS SuccessManagerName
                ,P.ClientHasTrackingTool
                FROM PROJECTS P
                JOIN CONSULTANT_DETAILS CD ON P.SuccessManagerId = CD.ConsultantId
                JOIN Users U ON CD.UserId = U.Id
                WHERE ProjectId = @ProjectId

            SELECT
                ProjectConsultantAssignedId
                ,ConsultantId
                ,HourlyClientRate
                ,HourlySalary
                ,MonthlyClientRate
                ,MonthlySalary
                ,PositionDetail
                FROM PROJECTS_CONSULTANTS_ASSIGNED WHERE ProjectId = @ProjectId
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PROJECTS_GetProjectDataById");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
