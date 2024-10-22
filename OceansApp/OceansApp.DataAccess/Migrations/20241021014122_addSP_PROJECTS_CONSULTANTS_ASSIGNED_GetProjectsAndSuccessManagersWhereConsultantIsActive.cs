using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSP_PROJECTS_CONSULTANTS_ASSIGNED_GetProjectsAndSuccessManagersWhereConsultantIsActive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_PROJECTS_CONSULTANTS_ASSIGNED_GetProjectsAndSuccessManagersWhereConsultantIsActive
             @ConsultantId INT
             AS
             BEGIN
             SET NOCOUNT ON;
             WITH LatestConsultantHistory AS (
                 SELECT 
                     ProjectConsultantAssignedId,
                     IsActive,
                     ROW_NUMBER() OVER (PARTITION BY ProjectConsultantAssignedId ORDER BY ActionDate DESC, Id DESC) AS RowNum
                 FROM 
                     PROJECTS_CONSULTANTS_ASSIGNED_HISTORY
                 WHERE 
                     ActionDate <= CAST(GETUTCDATE() AS DATE)
             ),
             LatestImageBlob AS (
                 SELECT 
                     EntityId,
                     BlobUrl,
                     ROW_NUMBER() OVER (PARTITION BY EntityId ORDER BY CreationDate DESC, BlobId DESC) AS RowNum
                 FROM 
                     IMAGE_BLOBS
                 WHERE 
                     EntityType = 'UserProfile'
             )
             SELECT 
                 pr.Name AS ProjectName,
                 CONCAT(u.Name, ' ', u.LastName) AS SuccessManagerName,
                 u.Email AS SuccessManagerEmail,
                 u.PhoneNumber AS SuccessManagerPhone,
                 lib.BlobUrl AS ProfileUrl
             FROM 
                 PROJECTS_CONSULTANTS_ASSIGNED pca
             INNER JOIN 
                 LatestConsultantHistory lch ON pca.ProjectConsultantAssignedId = lch.ProjectConsultantAssignedId
             INNER JOIN 
                 PROJECTS pr ON pca.ProjectId = pr.ProjectId
             INNER JOIN 
                 CONSULTANT_DETAILS cd ON pr.SuccessManagerId = cd.ConsultantId
             INNER JOIN 
                 Users U ON cd.UserId = u.Id
             LEFT JOIN 
                 LatestImageBlob lib ON cd.UserId = lib.EntityId AND lib.RowNum = 1  -- Selecciona solo el Blob más reciente
             WHERE 
                 lch.RowNum = 1  
             AND 
                 pca.ConsultantId = @ConsultantId
             AND 
                 lch.IsActive = 1
             ORDER BY pr.Name;
             END;
             ";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECTS_CONSULTANTS_ASSIGNED_GetProjectsAndSuccessManagersWhereConsultantIsActive");
        }
    }
}
