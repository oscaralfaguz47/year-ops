using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateSP_PROJECT_GetAssignedConsultantToProjectById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PROJECT_GetAssignedConsultantToProjectById
            @ProjectConsultantAssignedId INT
            AS
            BEGIN
            SELECT 
	        U.Name + ' ' + U.LastName AS ConsultantName
	        ,U.Email
            ,PCA.HourlyClientRate
            ,PCA.HourlySalary
            ,PCA.MonthlyClientRate
            ,PCA.MonthlySalary
            ,PCA.PositionDetail
            FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
            JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = PCA.ConsultantId
            JOIN Users U ON CD.UserId = U.Id
            WHERE PCA.ProjectConsultantAssignedId = @ProjectConsultantAssignedId
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECT_GetAssignedConsultantToProjectById");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_PROJECT_GetAssignedConsultantToProjectById
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

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PROJECT_GetAssignedConsultantToProjectById");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
