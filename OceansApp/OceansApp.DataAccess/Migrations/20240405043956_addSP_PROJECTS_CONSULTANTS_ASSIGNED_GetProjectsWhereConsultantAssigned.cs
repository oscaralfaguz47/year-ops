using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addSP_PROJECTS_CONSULTANTS_ASSIGNED_GetProjectsWhereConsultantAssigned : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_PROJECTS_CONSULTANTS_ASSIGNED_GetProjectsWhereConsultantAssigned
            @UserId NVARCHAR(450)
            AS
            BEGIN
            SELECT
            PCA.ProjectId
	        ,P.Name
            FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
            INNER JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
            INNER JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
            WHERE CD.UserId = @UserId
            ORDER BY P.Name;
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECTS_CONSULTANTS_ASSIGNED_GetProjectsWhereConsultantAssigned");
        }
    }
}
