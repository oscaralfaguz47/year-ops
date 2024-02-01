using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addSP_PROJECT_GetAssignedConsultantToProjectById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_PROJECT_GetAssignedConsultantToProjectById
            @ProjectConsultantAssignedId INT
            AS
            BEGIN
            SELECT 
              PCA.HourlyClientRate
              ,PCA.HourlySalary
              ,PCA.MonthlyClientRate
              ,PCA.MonthlySalary
              ,PCA.PositionDetail
              FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
              WHERE PCA.ProjectConsultantAssignedId = @ProjectConsultantAssignedId
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECT_GetAssignedConsultantToProjectById");
        }
    }
}
