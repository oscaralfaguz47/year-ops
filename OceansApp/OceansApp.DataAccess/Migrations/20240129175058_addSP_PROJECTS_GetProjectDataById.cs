using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addSP_PROJECTS_GetProjectDataById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_PROJECTS_GetProjectDataById
            @ProjectId INT
            AS
            BEGIN
            SELECT 
                ProjectId
                ,Name
                ,Description
                ,StartDate
                ,IsActive
                ,IsBillable
                ,ClientId
                ,SuccessManagerId
                ,ClientHasTrackingTool
                FROM PROJECTS WHERE ProjectId = @ProjectId

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
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECTS_GetProjectDataById");
        }
    }
}
