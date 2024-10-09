using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSP_PROJECTS_CONSULTANT_ASSIGNED_GetProjectsWhereActiveAndAccessToTrackingTool : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_PROJECTS_CONSULTANT_ASSIGNED_GetProjectsWhereActiveAndAccessToTrackingTool
             @ConsultantId INT
             AS
             BEGIN
                 SET NOCOUNT ON;
                 
                 ;WITH RecentHistory AS (
                     SELECT 
                         ProjectConsultantAssignedId, 
                         IsActive,
                         AccessToTrackingTool,
                         ROW_NUMBER() OVER (PARTITION BY ProjectConsultantAssignedId ORDER BY ActionDate DESC, Id DESC) AS RowNum
                     FROM 
                         PROJECTS_CONSULTANTS_ASSIGNED_HISTORY
                     WHERE 
                         ActionDate <= GETUTCDATE()
                 )
                 SELECT 
                     PCA.ProjectId
                 FROM 
                     PROJECTS_CONSULTANTS_ASSIGNED PCA
                 INNER JOIN 
                     RecentHistory RH ON PCA.ProjectConsultantAssignedId = RH.ProjectConsultantAssignedId 
                                     AND RH.RowNum = 1
                 INNER JOIN 
                     PROJECTS P ON PCA.ProjectId = P.ProjectId
                 WHERE 
                     PCA.ConsultantId = @ConsultantId
                     AND (RH.IsActive = 1 AND RH.AccessToTrackingTool = 1);
             END;
             ";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECTS_CONSULTANT_ASSIGNED_GetProjectsWhereActiveAndAccessToTrackingTool");
        }
    }
}
