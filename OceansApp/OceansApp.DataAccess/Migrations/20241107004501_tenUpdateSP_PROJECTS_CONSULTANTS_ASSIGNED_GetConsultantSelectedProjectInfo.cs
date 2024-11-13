using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class tenUpdateSP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo
            @UserId NVARCHAR(450)
            AS
            BEGIN
            SET NOCOUNT ON;
            SELECT 
                    CD.PaymentPeriod,
                    PUS.ProjectId,
                    P.Name AS ProjectName,
                    P.ClientHasTrackingTool,
                    CONCAT(SMU.Name, ' ', SMU.LastName) AS SuccessManagerName,
                    SMU.Email AS SuccessManagerEmail,
                    IB.BlobUrl,
                    ISNULL(CPC.NumAssignedProjects, 0) AS NumAssignedProjects
                FROM CONSULTANT_DETAILS CD
                LEFT JOIN PROJECTS_USERS_SELECTED PUS ON CD.UserId = PUS.UserId
                LEFT JOIN PROJECTS P ON PUS.ProjectId = P.ProjectId
                LEFT JOIN CONSULTANT_DETAILS SM ON P.SuccessManagerId = SM.ConsultantId
                LEFT JOIN Users SMU ON SM.UserId = SMU.Id
                LEFT JOIN IMAGE_BLOBS IB ON SMU.Id = IB.EntityId 
	            AND IB.ContainerName = 'user-profile-photos'
	            AND IB.EntityType = 'UserProfile'
                OUTER APPLY (
                    SELECT COUNT(*) AS NumAssignedProjects
                    FROM PROJECTS_CONSULTANTS_ASSIGNED
                    WHERE ConsultantId = CD.ConsultantId
                ) CPC
                WHERE CD.UserId = @UserId;
            END;";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo
            @UserId NVARCHAR(450)
            AS
            BEGIN
            SET NOCOUNT ON;
            SELECT 
                    CD.PaymentPeriod,
                    PUS.ProjectId,
                    P.Name AS ProjectName,
                    P.ClientHasTrackingTool,
                    CONCAT(SMU.Name, ' ', SMU.LastName) AS SuccessManagerName,
                    SMU.Email AS SuccessManagerEmail,
                    ISNULL(CPC.NumAssignedProjects, 0) AS NumAssignedProjects
                FROM CONSULTANT_DETAILS CD
                LEFT JOIN PROJECTS_USERS_SELECTED PUS ON CD.UserId = PUS.UserId
                LEFT JOIN PROJECTS P ON PUS.ProjectId = P.ProjectId
                LEFT JOIN CONSULTANT_DETAILS SM ON P.SuccessManagerId = SM.ConsultantId
                LEFT JOIN Users SMU ON SM.UserId = SMU.Id
                OUTER APPLY (
                    SELECT COUNT(*) AS NumAssignedProjects
                    FROM PROJECTS_CONSULTANTS_ASSIGNED
                    WHERE ConsultantId = CD.ConsultantId
                ) CPC
                WHERE CD.UserId = @UserId;
            END;";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
