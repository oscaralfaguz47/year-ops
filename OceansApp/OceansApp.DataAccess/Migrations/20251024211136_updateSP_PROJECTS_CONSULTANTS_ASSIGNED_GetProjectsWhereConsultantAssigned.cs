using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateSP_PROJECTS_CONSULTANTS_ASSIGNED_GetProjectsWhereConsultantAssigned : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PROJECTS_CONSULTANTS_ASSIGNED_GetProjectsWhereConsultantAssigned
            @UserId NVARCHAR(450)
            AS
            BEGIN
            WITH LatestHistory AS
            (
                SELECT
                    h.ProjectConsultantAssignedId,
                    h.IsActive,
                    rn = ROW_NUMBER() OVER
                         (
                             PARTITION BY h.ProjectConsultantAssignedId
                             ORDER BY h.ActionDate DESC, h.Id DESC
                         )
                FROM dbo.PROJECTS_CONSULTANTS_ASSIGNED_HISTORY AS h
            )
            SELECT
                PCA.ProjectId,
                P.Name
            FROM dbo.PROJECTS_CONSULTANTS_ASSIGNED AS PCA
            INNER JOIN LatestHistory AS LH
                ON LH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
               AND LH.rn = 1             
               AND LH.IsActive = 1        
            INNER JOIN dbo.CONSULTANT_DETAILS AS CD
                ON PCA.ConsultantId = CD.ConsultantId
            INNER JOIN dbo.PROJECTS AS P
                ON PCA.ProjectId = P.ProjectId
            WHERE CD.UserId = @UserId
            ORDER BY P.Name;
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECTS_CONSULTANTS_ASSIGNED_GetProjectsWhereConsultantAssigned");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_PROJECTS_CONSULTANTS_ASSIGNED_GetProjectsWhereConsultantAssigned
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

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PROJECTS_CONSULTANTS_ASSIGNED_GetProjectsWhereConsultantAssigned");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
